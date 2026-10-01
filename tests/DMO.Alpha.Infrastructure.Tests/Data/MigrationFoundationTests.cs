using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace DMO.Alpha.Infrastructure.Tests.Data;

/// <summary>
/// Prova a fundação de persistência durável do modelo ACTUAL: a primeira
/// migration EF Core (InitialCreate) representa exactamente o modelo corrente
/// do <see cref="DmoDbContext"/> — sem redesign de entidades, sem novos
/// campos, sem alteração de semântica — e gera SQL PostgreSQL válido com as
/// constraints canónicas já codificadas no modelo:
/// relações JobOn/contextos (1:1 por JobOnId), PK da Tool (tool_id),
/// BqRepairTrace → Tool obrigatória, BqRepairTrace → BqContext opcional com
/// bq_id único no trace, BqMovement → BqRepairTrace obrigatória e o storage
/// corrente do tipo de movimento por tokens textuais.
/// </summary>
public sealed class MigrationFoundationTests
{
    // Ligação nunca contactada: os testes de migration operam apenas sobre o
    // modelo e as operations, sem servidor PostgreSQL.
    private const string TestConnectionString =
        "Host=localhost;Database=dmo_alpha_test;Username=dmo;Password=dmo";

    private static DmoDbContext CreateNpgsqlContext()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseNpgsql(TestConnectionString)
            .Options;

        return new DmoDbContext(options);
    }

    private static Migration CreateInitialMigration(DmoDbContext db)
    {
        var migrationsAssembly = db.GetService<IMigrationsAssembly>();
        var (id, type) = Assert.Single(migrationsAssembly.Migrations);
        Assert.EndsWith("_InitialCreate", id);

        return migrationsAssembly.CreateMigration(type, db.Database.ProviderName!);
    }

    [Fact]
    public void Migrations_ContainExactlyTheFirstInitialMigration()
    {
        using var db = CreateNpgsqlContext();

        var migrations = db.GetService<IMigrationsAssembly>().Migrations;

        var entry = Assert.Single(migrations);
        Assert.Equal("InitialCreate", entry.Value.Name);
        Assert.Matches(@"^\d{14}_InitialCreate$", entry.Key);
    }

    [Fact]
    public void InitialCreate_TargetModel_MatchesCurrentModelExactly()
    {
        // A prova de exactidão: o model snapshot da migration é indistinguível
        // do modelo Npgsql corrente do DmoDbContext — qualquer diferença
        // (campo novo, coluna removida, constraint alterada) falha aqui.
        using var db = CreateNpgsqlContext();

        var migrationsAssembly = db.GetService<IMigrationsAssembly>();
        Assert.NotNull(migrationsAssembly.ModelSnapshot);

        // O snapshot é um modelo mutável stand-alone; é finalizado e
        // inicializado em modo design-time antes da comparação, e o modelo
        // corrente entra pela forma design-time (IDesignTimeModel) — o
        // differ exige-a para aceder a configuração não optimizada.
        var initializer = db.GetService<IModelRuntimeInitializer>();
        var snapshotRawModel = migrationsAssembly.ModelSnapshot!.Model;
        var snapshotModel = snapshotRawModel is IMutableModel mutableModel
            ? initializer.Initialize(mutableModel.FinalizeModel(), designTime: true)
            : initializer.Initialize(snapshotRawModel, designTime: true);

        var currentModel = db.GetService<IDesignTimeModel>().Model;

        var differ = db.GetService<IMigrationsModelDiffer>();

        var differences = differ.GetDifferences(
            snapshotModel.GetRelationalModel(),
            currentModel.GetRelationalModel());

        Assert.Empty(differences);
    }

    [Fact]
    public void InitialCreate_UpOperations_CreateExactlyTheCanonicalTables()
    {
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var createdTables = migration.UpOperations
            .OfType<CreateTableOperation>()
            .Select(t => t.Name)
            .ToHashSet();

        // Exactamente as 10 tabelas do modelo corrente: identidade/auth já
        // persistida, produção (Machine/JobOn/contextos), registo canónico de
        // Tools e Boquilhas (trace/movimentos). Nada mais, nada menos.
        Assert.Equal(
            new HashSet<string>
            {
                "Users",
                "AdminAssociations",
                "Machines",
                "JobOns",
                "CmContexts",
                "MfContexts",
                "BqContexts",
                "Tools",
                "BqRepairTraces",
                "BqMovements"
            },
            createdTables);

        // Down reverte exactamente as mesmas tabelas.
        Assert.All(
            migration.DownOperations,
            o => Assert.IsType<DropTableOperation>(o));
        Assert.Equal(
            createdTables,
            migration.DownOperations
                .OfType<DropTableOperation>()
                .Select(o => o.Name)
                .ToHashSet());
    }

    [Fact]
    public void InitialCreate_EncodesJobOnAndContextRelationships()
    {
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);
        var tables = migration.UpOperations.OfType<CreateTableOperation>()
            .ToDictionary(t => t.Name);

        var jobOns = tables["JobOns"];

        // JobOn → Machine obrigatória, cascade (constraint actual do modelo).
        var machineFk = Assert.Single(jobOns.ForeignKeys);
        Assert.Equal(["MachineId"], machineFk.Columns);
        Assert.Equal("Machines", machineFk.PrincipalTable);
        Assert.False(jobOns.Columns.Single(c => c.Name == "MachineId").IsNullable);
        Assert.Equal(ReferentialAction.Cascade, machineFk.OnDelete);

        // JobOn ← contextos: um JobOnId por contexto (1:1) — FK obrigatória
        // com índice único sobre JobOnId, ToolId textual como âncora.
        foreach (var (tableName, indexName) in new[]
                 {
                     ("CmContexts", "IX_CmContexts_JobOnId"),
                     ("MfContexts", "IX_MfContexts_JobOnId"),
                     ("BqContexts", "IX_BqContexts_JobOnId")
                 })
        {
            var contextTable = tables[tableName];

            var jobOnFk = Assert.Single(contextTable.ForeignKeys);
            Assert.Equal(["JobOnId"], jobOnFk.Columns);
            Assert.Equal("JobOns", jobOnFk.PrincipalTable);
            Assert.False(contextTable.Columns.Single(c => c.Name == "JobOnId").IsNullable);
            Assert.Equal(ReferentialAction.Cascade, jobOnFk.OnDelete);

            var toolIdColumn = contextTable.Columns.Single(c => c.Name == "ToolId");
            Assert.Equal(typeof(string), toolIdColumn.ClrType);
            Assert.False(toolIdColumn.IsNullable);

            // O 1:1 materializa-se no índice único sobre o FK do contexto.
            var jobOnIndex = migration.UpOperations.OfType<CreateIndexOperation>()
                .Single(i => i.Name == indexName);
            Assert.Equal(tableName, jobOnIndex.Table);
            Assert.True(jobOnIndex.IsUnique);
        }

        // ProductionNumber do JobOn permanece único, como hoje.
        var productionNumberIndex = migration.UpOperations.OfType<CreateIndexOperation>()
            .Single(i => i.Table == "JobOns" && i.Columns.SequenceEqual(["ProductionNumber"]));
        Assert.True(productionNumberIndex.IsUnique);
    }

    [Fact]
    public void InitialCreate_EncodesCanonicalToolPrimaryKey()
    {
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var tools = migration.UpOperations.OfType<CreateTableOperation>()
            .Single(t => t.Name == "Tools");

        // A identidade da Tool é o tool_id emitido pelo backend (PK textual);
        // nenhuma unicidade sobre factos de descoberta (referência/lote).
        Assert.Equal(["ToolId"], tools.PrimaryKey!.Columns);
        var toolIdColumn = tools.Columns.Single(c => c.Name == "ToolId");
        Assert.Equal(typeof(string), toolIdColumn.ClrType);
        Assert.False(toolIdColumn.IsNullable);

        // O tipo persiste por tokens textuais (CM/MF/BQ), como hoje.
        var typeColumn = tools.Columns.Single(c => c.Name == "Type");
        Assert.Equal(typeof(string), typeColumn.ClrType);
        Assert.False(typeColumn.IsNullable);

        // Referência e lote são apenas atributos: colunas required sem índice.
        Assert.False(tools.Columns.Single(c => c.Name == "Reference").IsNullable);
        Assert.False(tools.Columns.Single(c => c.Name == "Lot").IsNullable);
        Assert.DoesNotContain(
            migration.UpOperations.OfType<CreateIndexOperation>(),
            i => i.Table == "Tools");
    }

    [Fact]
    public void InitialCreate_EncodesTraceAnchors_OptionalBqContext_UniqueBqContextId()
    {
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var traces = migration.UpOperations.OfType<CreateTableOperation>()
            .Single(t => t.Name == "BqRepairTraces");

        // BqRepairTrace → Tool: âncora permanente obrigatória (tool_id).
        var toolFk = traces.ForeignKeys.Single(f => f.PrincipalTable == "Tools");
        Assert.Equal(["ToolId"], toolFk.Columns);
        Assert.Equal(["ToolId"], toolFk.PrincipalColumns!);
        Assert.False(traces.Columns.Single(c => c.Name == "ToolId").IsNullable);
        Assert.Equal(ReferentialAction.Cascade, toolFk.OnDelete);

        // BqRepairTrace → BqContext: opcional (bq_id não resolvido é null).
        var bqFk = traces.ForeignKeys.Single(f => f.PrincipalTable == "BqContexts");
        Assert.Equal(["BqContextId"], bqFk.Columns);
        Assert.Equal(["Id"], bqFk.PrincipalColumns!);
        Assert.True(traces.Columns.Single(c => c.Name == "BqContextId").IsNullable);
        Assert.Equal(ReferentialAction.NoAction, bqFk.OnDelete);

        // A única unicidade decidida: um bq_id tem um único trace.
        var bqContextIndex = migration.UpOperations.OfType<CreateIndexOperation>()
            .Single(i => i.Table == "BqRepairTraces" && i.Columns.SequenceEqual(["BqContextId"]));
        Assert.True(bqContextIndex.IsUnique);

        // Nenhuma regra de unicidade sobre ToolId: a cardinalidade de traces
        // pendentes por tool_id permanece deliberadamente em aberto.
        var toolIdIndex = migration.UpOperations.OfType<CreateIndexOperation>()
            .Single(i => i.Table == "BqRepairTraces" && i.Columns.SequenceEqual(["ToolId"]));
        Assert.False(toolIdIndex.IsUnique);
    }

    [Fact]
    public void InitialCreate_EncodesMovementBelongsToTraceAndCurrentTypeStorage()
    {
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var movements = migration.UpOperations.OfType<CreateTableOperation>()
            .Single(t => t.Name == "BqMovements");

        // BqMovement → BqRepairTrace obrigatória: cada movimento pertence a
        // um bq_repair_trace_id.
        var traceFk = Assert.Single(movements.ForeignKeys);
        Assert.Equal(["BqRepairTraceId"], traceFk.Columns);
        Assert.Equal("BqRepairTraces", traceFk.PrincipalTable);
        Assert.False(movements.Columns.Single(c => c.Name == "BqRepairTraceId").IsNullable);
        Assert.Equal(ReferentialAction.Cascade, traceFk.OnDelete);

        // Storage corrente do tipo de movimento: tokens textuais
        // (saida/entrada/entrada_sem_reparacao), não inteiro de enum.
        var typeColumn = movements.Columns.Single(c => c.Name == "Type");
        Assert.Equal(typeof(string), typeColumn.ClrType);
        Assert.False(typeColumn.IsNullable);

        // Quantidade observada obrigatória na totalidade; discrepância é facto
        // histórico opcional (null = sem discrepância).
        Assert.False(movements.Columns.Single(c => c.Name == "Quantity").IsNullable);
        Assert.True(movements.Columns.Single(c => c.Name == "Discrepancy").IsNullable);
    }

    [Fact]
    public void InitialCreate_PreservesIdentityAuthPersistence()
    {
        // A persistência de identidade/auth já presente no DmoDbContext
        // (Users, AdminAssociations) entra na migration tal como está.
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var users = migration.UpOperations.OfType<CreateTableOperation>()
            .Single(t => t.Name == "Users");
        Assert.Equal(["Id"], users.PrimaryKey!.Columns);
        var operatorIndex = migration.UpOperations.OfType<CreateIndexOperation>()
            .Single(i => i.Table == "Users" && i.Columns.SequenceEqual(["OperatorId"]));
        Assert.True(operatorIndex.IsUnique);
        var operatorIdColumn = users.Columns.Single(c => c.Name == "OperatorId");
        Assert.Equal(4, operatorIdColumn.MaxLength);

        var adminAssociations = migration.UpOperations.OfType<CreateTableOperation>()
            .Single(t => t.Name == "AdminAssociations");
        Assert.Equal(["Id"], adminAssociations.PrimaryKey!.Columns);
        var emailIndex = migration.UpOperations.OfType<CreateIndexOperation>()
            .Single(i => i.Table == "AdminAssociations" && i.Columns.SequenceEqual(["Email"]));
        Assert.True(emailIndex.IsUnique);
    }

    [Fact]
    public void InitialGenerate_GeneratesValidPostgreSqlWithCanonicalConstraints()
    {
        // As operations da migration, compiladas para o dialeto PostgreSQL,
        // produzem exactamente o DDL canónico corrente (sem servidor).
        using var db = CreateNpgsqlContext();
        var migration = CreateInitialMigration(db);

        var sqlGenerator = db.GetService<IMigrationsSqlGenerator>();
        var commands = sqlGenerator.Generate(migration.UpOperations, db.Model);
        var sql = string.Join("\n", commands.Select(c => c.CommandText));

        // Tabelas canónicas.
        foreach (var table in new[]
                 {
                     "AdminAssociations", "Users", "Machines", "JobOns",
                     "CmContexts", "MfContexts", "BqContexts",
                     "Tools", "BqRepairTraces", "BqMovements"
                 })
        {
            Assert.Contains($"CREATE TABLE \"{table}\"", sql);
        }

        // Relações JobOn/contextos: FK obrigatória + índice único 1:1.
        Assert.Contains(
            "CONSTRAINT \"FK_JobOns_Machines_MachineId\" FOREIGN KEY (\"MachineId\") REFERENCES \"Machines\" (\"Id\") ON DELETE CASCADE",
            sql);
        foreach (var context in new[] { "CmContexts", "MfContexts", "BqContexts" })
        {
            Assert.Contains(
                $"CONSTRAINT \"FK_{context}_JobOns_JobOnId\" FOREIGN KEY (\"JobOnId\") REFERENCES \"JobOns\" (\"Id\") ON DELETE CASCADE",
                sql);
            Assert.Contains(
                $"CREATE UNIQUE INDEX \"IX_{context}_JobOnId\" ON \"{context}\" (\"JobOnId\")",
                sql);
        }

        // PK da Tool (tool_id textual).
        Assert.Contains("CONSTRAINT \"PK_Tools\" PRIMARY KEY (\"ToolId\")", sql);
        Assert.Contains("\"ToolId\" text NOT NULL", sql);

        // Trace → Tool obrigatória (cascade) e → BqContext opcional (coluna
        // nullable, sem cascade).
        Assert.Contains(
            "CONSTRAINT \"FK_BqRepairTraces_Tools_ToolId\" FOREIGN KEY (\"ToolId\") REFERENCES \"Tools\" (\"ToolId\") ON DELETE CASCADE",
            sql);
        Assert.Contains(
            "CONSTRAINT \"FK_BqRepairTraces_BqContexts_BqContextId\" FOREIGN KEY (\"BqContextId\") REFERENCES \"BqContexts\" (\"Id\")",
            sql);
        Assert.DoesNotContain("\"BqContextId\" integer NOT NULL", sql);

        // Um bq_id tem um único trace; nenhum índice único sobre ToolId.
        Assert.Contains(
            "CREATE UNIQUE INDEX \"IX_BqRepairTraces_BqContextId\" ON \"BqRepairTraces\" (\"BqContextId\")",
            sql);
        Assert.DoesNotContain("CREATE UNIQUE INDEX \"IX_BqRepairTraces_ToolId\"", sql);

        // Movimento → trace obrigatória; tokens textuais persistem como text.
        Assert.Contains(
            "CONSTRAINT \"FK_BqMovements_BqRepairTraces_BqRepairTraceId\" FOREIGN KEY (\"BqRepairTraceId\") REFERENCES \"BqRepairTraces\" (\"Id\") ON DELETE CASCADE",
            sql);
        var movementsTable = TableFragment(sql, "BqMovements");
        Assert.Contains("\"Type\" text NOT NULL", movementsTable);
        Assert.Contains("\"Quantity\" integer NOT NULL", movementsTable);
        Assert.DoesNotContain("\"Discrepancy\" integer NOT NULL", movementsTable);
    }

    private static string TableFragment(string sql, string table)
    {
        var start = sql.IndexOf($"CREATE TABLE \"{table}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"CREATE TABLE \"{table}\" não encontrado no SQL gerado.");

        var end = sql.IndexOf(");", start, StringComparison.Ordinal);
        Assert.True(end > start, $"Fim da tabela \"{table}\" não encontrado no SQL gerado.");

        return sql[start..end];
    }
}
