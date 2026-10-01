using System.Data;
using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Infrastructure.Tools;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Data;

/// <summary>
/// Aplica a fundação de persistência (migration InitialCreate) contra um
/// PostgreSQL real — Supabase incluído — quando a ligação de teste está
/// configurada através da variável de ambiente DMO_POSTGRES_CONNECTION.
/// Aponte a uma base de dados de teste descartável: os testes aplicam a
/// migration e criam/apagam apenas os seus próprios registos.
/// Sem ligação configurada, os testes saltam — nunca inventam credenciais
/// nem alvos de base de dados.
/// </summary>
public sealed class PostgresMigrationIntegrationTests
{
    private static string? Connection => Environment.GetEnvironmentVariable("DMO_POSTGRES_CONNECTION");

    private static DmoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseNpgsql(Connection!)
            .Options;

        return new DmoDbContext(options);
    }

    private static void SkipWithoutConnection()
        => Skip.If(
            string.IsNullOrWhiteSpace(Connection),
            "DMO_POSTGRES_CONNECTION is not configured (real PostgreSQL/Supabase test database).");

    [SkippableFact]
    public async Task Migrate_AppliesInitialCreate_AndCreatesTheCanonicalSchema()
    {
        SkipWithoutConnection();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();

        // A migration inicial é a única entrada aplicada na história.
        var applied = await db.Database.GetAppliedMigrationsAsync();
        var entry = Assert.Single(applied);
        Assert.EndsWith("_InitialCreate", entry);

        // Um bq_id tem um único trace: índice único persistido no PostgreSQL.
        var traceIndex = await db.Database
            .SqlQuery<string>($"""
                SELECT indexdef AS "Value" FROM pg_indexes
                WHERE schemaname = current_schema()
                  AND tablename = 'BqRepairTraces'
                  AND indexname = 'IX_BqRepairTraces_BqContextId'
                """)
            .ToListAsync();
        Assert.Single(traceIndex);
        Assert.Contains("UNIQUE", traceIndex[0]);
        Assert.Contains("\"BqContextId\"", traceIndex[0]);

        // Nenhuma unicidade sobre ToolId: a cardinalidade de traces pendentes
        // por tool_id permanece em aberto, como no modelo corrente.
        var toolIndex = await db.Database
            .SqlQuery<string>($"""
                SELECT indexdef AS "Value" FROM pg_indexes
                WHERE schemaname = current_schema()
                  AND tablename = 'BqRepairTraces'
                  AND indexname = 'IX_BqRepairTraces_ToolId'
                """)
            .ToListAsync();
        Assert.Single(toolIndex);
        Assert.DoesNotContain("UNIQUE", toolIndex[0]);

        // PK da Tool é o tool_id textual emitido pelo backend.
        var toolPrimaryKey = await db.Database
            .SqlQuery<string>($"""
                SELECT constraint_type AS "Value" FROM information_schema.table_constraints
                WHERE table_schema = current_schema()
                  AND constraint_name = 'PK_Tools'
                """)
            .ToListAsync();
        Assert.Equal(["PRIMARY KEY"], toolPrimaryKey);

        // Storage corrente do tipo de movimento: tokens textuais.
        var movementType = await db.Database
            .SqlQuery<string>($"""
                SELECT data_type AS "Value" FROM information_schema.columns
                WHERE table_schema = current_schema()
                  AND table_name = 'BqMovements'
                  AND column_name = 'Type'
                """)
            .ToListAsync();
        Assert.Equal(["text"], movementType);

        var movementTypeNullability = await db.Database
            .SqlQuery<string>($"""
                SELECT is_nullable AS "Value" FROM information_schema.columns
                WHERE table_schema = current_schema()
                  AND table_name = 'BqMovements'
                  AND column_name = 'Type'
                """)
            .ToListAsync();
        Assert.Equal(["NO"], movementTypeNullability);

        // Discrepância: facto histórico opcional (null = sem discrepância).
        var discrepancyNullability = await db.Database
            .SqlQuery<string>($"""
                SELECT is_nullable AS "Value" FROM information_schema.columns
                WHERE table_schema = current_schema()
                  AND table_name = 'BqMovements'
                  AND column_name = 'Discrepancy'
                """)
            .ToListAsync();
        Assert.Equal(["YES"], discrepancyNullability);

        // Trace → Tool obrigatória e trace → BqContext opcional existem
        // exactamente como constraints de base de dados.
        var traceForeignKeys = await db.Database
            .SqlQuery<string>($"""
                SELECT constraint_name AS "Value" FROM information_schema.table_constraints
                WHERE table_schema = current_schema()
                  AND table_name = 'BqRepairTraces'
                  AND constraint_type = 'FOREIGN KEY'
                """)
            .ToListAsync();
        Assert.Contains("FK_BqRepairTraces_Tools_ToolId", traceForeignKeys);
        Assert.Contains("FK_BqRepairTraces_BqContexts_BqContextId", traceForeignKeys);
        Assert.Equal(2, traceForeignKeys.Count);
    }

    [SkippableFact]
    public async Task WritePath_AgainstPostgres_MatchesCanonicalModelSemantics()
    {
        SkipWithoutConnection();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();

        var createHandler = new CreateBqRepairTraceHandler(db);
        var movementHandler = new RegisterBqMovementHandler(db);
        var associateHandler = new AssociateBqRepairTraceToContextHandler(db);

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var toolId = $"BQ-TOOL-PG-{suffix}";

        // Registo canónico da Tool (tool_id emitido pelo backend).
        db.Tools.Add(new Tool
        {
            ToolId = toolId,
            Type = ToolType.Bq,
            Reference = "PG-WRITE-PATH",
            Lot = "1"
        });
        await db.SaveChangesAsync();

        try
        {
            // Trace pré-produção ancorado ao tool_id, bq_id não resolvido.
            var created = await createHandler.HandleAsync(new CreateBqRepairTrace(toolId));
            Assert.Equal(CreateBqRepairTraceResult.Success, created.Result);
            var traceId = created.Trace!.BqRepairTraceId;

            // Movimentos com a quantidade observada na totalidade e a
            // discrepância produzida como facto histórico.
            var saida = await movementHandler.HandleAsync(new RegisterBqMovement(traceId, "saida", 5));
            Assert.Equal(RegisterBqMovementResult.Success, saida.Result);

            var entrada = await movementHandler.HandleAsync(new RegisterBqMovement(traceId, "entrada", 7));
            Assert.Equal(RegisterBqMovementResult.Success, entrada.Result);
            Assert.Equal(7, entrada.Movement!.Quantity);
            Assert.Equal(-2, entrada.Movement.Discrepancy);

            // Contexto de produção BQ.
            var machine = new Machine { Code = $"PG-{suffix}" };
            var jobOn = new JobOn
            {
                Reference = "PG-WRITE-PATH",
                ProductionNumber = $"PG-PN-{suffix}",
                Machine = machine
            };
            var bq = new BqContext { JobOn = jobOn, ToolId = toolId };
            db.JobOns.Add(jobOn);
            db.BqContexts.Add(bq);
            await db.SaveChangesAsync();

            // O mesmo trace passa a referenciar o seu bq_id.
            var associated = await associateHandler.HandleAsync(
                new AssociateBqRepairTraceToContext(traceId, bq.Id));
            Assert.Equal(AssociateBqRepairTraceToContextResult.Success, associated.Result);

            // Um bq_id tem um único bq_repair_trace_id: recusa tipada limpa
            // para um segundo trace da mesma Tool.
            var second = await createHandler.HandleAsync(new CreateBqRepairTrace(toolId));
            Assert.Equal(CreateBqRepairTraceResult.Success, second.Result);
            var refused = await associateHandler.HandleAsync(
                new AssociateBqRepairTraceToContext(second.Trace!.BqRepairTraceId, bq.Id));
            Assert.Equal(AssociateBqRepairTraceToContextResult.BqContextAlreadyTraced, refused.Result);

            // Os tokens persistem como texto canónico no PostgreSQL.
            var storedTypes = await db.Database
                .SqlQuery<string>(
                    $"SELECT \"Type\" AS \"Value\" FROM \"BqMovements\" WHERE \"BqRepairTraceId\" = {traceId} ORDER BY \"Id\"")
                .ToListAsync();
            Assert.Equal(["saida", "entrada"], storedTypes);

            // Leitura relacional: o movimento resolve o trace e, através
            // dele, o tool_id canónico e o bq_id de produção.
            var resolved = await db.BqMovements
                .AsNoTracking()
                .Include(m => m.Trace)
                .ThenInclude(t => t.Tool)
                .SingleAsync(m => m.Id == entrada.Movement.MovementId);
            Assert.Equal(traceId, resolved.BqRepairTraceId);
            Assert.Equal(toolId, resolved.Trace.Tool.ToolId);
            Assert.Equal(bq.Id, resolved.Trace.BqContextId);
        }
        finally
        {
            // Limpeza em ordem de dependência: a Tool arrasta os traces e os
            // movimentos (cascade); a Machine arrasta o JobOn e o BqContext.
            var tool = await db.Tools.SingleAsync(t => t.ToolId == toolId);
            db.Tools.Remove(tool);

            var machine = await db.Machines.SingleAsync(m => m.Code == $"PG-{suffix}");
            db.Machines.Remove(machine);

            await db.SaveChangesAsync();
        }
    }

    [SkippableFact]
    public async Task ToolLookupQuery_AgainstPostgres_RoundTripsProjectionAndToolType()
    {
        SkipWithoutConnection();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var toolId = $"BQ-TOOL-PG-LOOKUP-{suffix}";

        // Registo canónico da Tool com o tipo convertido em valor (ToolType).
        db.Tools.Add(new Tool
        {
            ToolId = toolId,
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await db.SaveChangesAsync();

        try
        {
            // Prova executada da projeção do read model contra o único
            // provider relacional de produção (Npgsql), incluindo o
            // round-trip do ToolType convertido em valor.
            var result = await new ToolLookupQuery(db).FindByToolIdAsync(toolId);

            Assert.NotNull(result);
            Assert.Equal(toolId, result.ToolId);
            Assert.Equal(ToolType.Bq, result.Type);
            Assert.Equal("9389T194", result.Reference);
            Assert.Equal("12", result.Lot);
        }
        finally
        {
            // Limpeza: apaga apenas a Tool semeada por este teste.
            var tool = await db.Tools.SingleAsync(t => t.ToolId == toolId);
            db.Tools.Remove(tool);

            await db.SaveChangesAsync();
        }
    }
}
