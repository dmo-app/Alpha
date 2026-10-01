using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Boquilhas;

/// <summary>
/// Prova as âncoras canónicas do registo de reparação de Boquilhas:
/// o trace resolve o tool_id canónico; pode existir antes do Job On com
/// bq_id não resolvido (sem regra de "um trace pendente por Tool");
/// pode mais tarde referenciar o seu bq_id sem mudar a identidade da Tool,
/// do trace ou do contexto de produção; a mesma Tool pode ter vários
/// traces de produção ao longo do tempo (um por bq_id).
/// </summary>
public sealed class BqRepairTraceIntegrationTests : IDisposable
{
    private readonly DmoDbContext _db;

    public BqRepairTraceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new DmoDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task<Tool> SeedBqToolAsync(string toolId)
    {
        var tool = new Tool
        {
            ToolId = toolId,
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        };
        _db.Tools.Add(tool);
        await _db.SaveChangesAsync();
        return tool;
    }

    private async Task<BqContext> SeedProductionBqContextAsync(
        string toolId,
        string productionNumber)
    {
        var machine = new Machine { Code = "B1" };
        var jobOn = new JobOn
        {
            Reference = "9389T194",
            ProductionNumber = productionNumber,
            Machine = machine
        };
        var bq = new BqContext { JobOn = jobOn, ToolId = toolId };

        _db.JobOns.Add(jobOn);
        _db.BqContexts.Add(bq);
        await _db.SaveChangesAsync();
        return bq;
    }

    [Fact]
    public async Task Trace_ResolvesCanonicalToolId()
    {
        var tool = await SeedBqToolAsync("BQ-TOOL-9");

        var trace = new BqRepairTrace { ToolId = "BQ-TOOL-9" };
        _db.BqRepairTraces.Add(trace);
        await _db.SaveChangesAsync();

        var resolved = await _db.BqRepairTraces
            .AsNoTracking()
            .Include(t => t.Tool)
            .SingleAsync(t => t.Id == trace.Id);

        Assert.Equal("BQ-TOOL-9", resolved.ToolId);
        Assert.Equal(tool.ToolId, resolved.Tool.ToolId);
        Assert.Equal(ToolType.Bq, resolved.Tool.Type);
        Assert.Equal("9389T194", resolved.Tool.Reference);
        Assert.Equal("12", resolved.Tool.Lot);
    }

    [Fact]
    public async Task PreProductionTraces_PersistWithoutBqContext_NoOnePendingTraceRule()
    {
        // Um trace pode existir antes do Job On, com bq_id não resolvido.
        // A cardinalidade de traces pendentes simultâneos por tool_id está
        // por decidir no blueprint: a persistência não impõe a regra
        // "um trace pendente por Tool" — dois pendentes da mesma Tool
        // persistem sem erro.
        await SeedBqToolAsync("BQ-TOOL-9");

        var first = new BqRepairTrace { ToolId = "BQ-TOOL-9" };
        var second = new BqRepairTrace { ToolId = "BQ-TOOL-9" };
        _db.BqRepairTraces.Add(first);
        _db.BqRepairTraces.Add(second);
        await _db.SaveChangesAsync();

        var pending = await _db.BqRepairTraces
            .AsNoTracking()
            .Where(t => t.ToolId == "BQ-TOOL-9")
            .ToListAsync();

        Assert.Equal(2, pending.Count);
        Assert.All(pending, t => Assert.Null(t.BqContextId));
        Assert.Equal(2, pending.Select(t => t.Id).Distinct().Count());
    }

    [Fact]
    public async Task Association_SetsBqContext_WithoutChangingTraceToolOrContextIdentity()
    {
        await SeedBqToolAsync("BQ-TOOL-9");

        var trace = new BqRepairTrace { ToolId = "BQ-TOOL-9" };
        _db.BqRepairTraces.Add(trace);
        await _db.SaveChangesAsync();

        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");
        var bqJobOnId = bq.JobOnId;

        // Associação: o mesmo trace passa a referenciar o seu bq_id.
        var stored = await _db.BqRepairTraces.SingleAsync(t => t.Id == trace.Id);
        stored.BqContextId = bq.Id;
        await _db.SaveChangesAsync();

        var resolved = await _db.BqRepairTraces
            .AsNoTracking()
            .Include(t => t.Tool)
            .Include(t => t.BqContext!)
            .ThenInclude(c => c.JobOn)
            .SingleAsync(t => t.Id == trace.Id);

        // O trace mantém a identidade e a âncora da Tool física.
        Assert.Equal(trace.Id, resolved.Id);
        Assert.Equal("BQ-TOOL-9", resolved.ToolId);
        Assert.Equal("BQ-TOOL-9", resolved.Tool.ToolId);
        Assert.Equal(ToolType.Bq, resolved.Tool.Type);

        // O trace carrega o contexto de produção bq_id.
        Assert.Equal(bq.Id, resolved.BqContextId);
        Assert.NotNull(resolved.BqContext);
        Assert.Equal(bq.Id, resolved.BqContext!.Id);

        // O contexto de produção existente não foi mutado pela associação.
        var contextAfter = await _db.BqContexts
            .AsNoTracking()
            .Include(c => c.JobOn)
            .SingleAsync(c => c.Id == bq.Id);
        Assert.Equal("BQ-TOOL-9", contextAfter.ToolId);
        Assert.Equal(bqJobOnId, contextAfter.JobOnId);
        Assert.Equal("PN-202609", contextAfter.JobOn!.ProductionNumber);
    }

    [Fact]
    public async Task SameTool_ManyProductionTracesAcrossTime()
    {
        // A mesma Tool física pode ter muitos traces ao longo do tempo:
        // cada produção recebe o seu próprio bq_id e o seu próprio trace.
        await SeedBqToolAsync("BQ-TOOL-9");

        var firstBq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");
        var secondBq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202610");

        _db.BqRepairTraces.Add(new BqRepairTrace { ToolId = "BQ-TOOL-9", BqContextId = firstBq.Id });
        _db.BqRepairTraces.Add(new BqRepairTrace { ToolId = "BQ-TOOL-9", BqContextId = secondBq.Id });
        await _db.SaveChangesAsync();

        var traces = await _db.BqRepairTraces
            .AsNoTracking()
            .Where(t => t.ToolId == "BQ-TOOL-9")
            .OrderBy(t => t.Id)
            .ToListAsync();

        Assert.Equal(2, traces.Count);
        Assert.All(traces, t => Assert.Equal("BQ-TOOL-9", t.ToolId));
        Assert.Equal(
            new int?[] { firstBq.Id, secondBq.Id },
            traces.Select(t => t.BqContextId).ToArray());
        Assert.Equal(2, traces.Select(t => t.Id).Distinct().Count());
    }

    [Fact]
    public void ModelShape_OneTracePerBqContextUnique_NoToolIdUniqueness()
    {
        // Relação canónica decidida: um bq_id tem um único trace — índice
        // único apenas sobre a âncora bq_id. Nenhuma unicidade sobre
        // ToolId: a cardinalidade de traces pendentes por tool_id está
        // deliberadamente em aberto.
        var entityType = _db.Model.FindEntityType(typeof(BqRepairTrace));
        Assert.NotNull(entityType);

        var bqContextIndex = entityType!.GetIndexes()
            .SingleOrDefault(i => i.Properties.Count == 1
                && i.Properties[0].Name == nameof(BqRepairTrace.BqContextId));
        Assert.NotNull(bqContextIndex);
        Assert.True(bqContextIndex!.IsUnique);

        Assert.DoesNotContain(
            entityType.GetIndexes(),
            i => i.IsUnique && i.Properties.Any(p => p.Name == nameof(BqRepairTrace.ToolId)));
    }

    [Fact]
    public void RelationalSchema_UniqueIndexOnBqContextId_Only()
    {
        // Confirma no SQL relacional (Npgsql, sem servidor) que existe
        // exatamente a unicidade decidida (um trace por bq_id) e nenhuma
        // unicidade sobre ToolId.
        var relationalOptions = new DbContextOptionsBuilder<DmoDbContext>()
            .UseNpgsql("Host=localhost;Database=dmo_alpha_test;Username=dmo;Password=dmo")
            .Options;

        using var relationalDb = new DmoDbContext(relationalOptions);
        var sql = relationalDb.Database.GenerateCreateScript();

        Assert.Contains("BqRepairTraces", sql);
        Assert.Contains("UNIQUE INDEX \"IX_BqRepairTraces_BqContextId\"", sql);
        Assert.DoesNotContain("UNIQUE INDEX \"IX_BqRepairTraces_ToolId\"", sql);
    }
}
