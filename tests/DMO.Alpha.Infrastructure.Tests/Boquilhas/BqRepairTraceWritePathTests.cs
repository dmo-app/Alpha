using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Boquilhas;

/// <summary>
/// Prova o write path do registo de reparação de Boquilhas: criação de um
/// bq_repair_trace_id ancorado a um tool_id canónico (bq_id não resolvido,
/// sem inferir Job On, múltiplos traces não resolvidos por tool_id
/// coexistem) e associação posterior do mesmo trace ao seu bq_id
/// (preserva identidade, tool_id e movimentos; não altera JobOn nem
/// BqContext; respeita um-trace-por-bq_id; não reassocia nem mistura
/// tool_ids).
/// </summary>
public sealed class BqRepairTraceWritePathTests : IDisposable
{
    private readonly DmoDbContext _db;
    private readonly CreateBqRepairTraceHandler _createHandler;
    private readonly AssociateBqRepairTraceToContextHandler _associateHandler;
    private readonly RegisterBqMovementHandler _movementHandler;

    public BqRepairTraceWritePathTests()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new DmoDbContext(options);
        _db.Database.EnsureCreated();
        _createHandler = new CreateBqRepairTraceHandler(_db);
        _associateHandler = new AssociateBqRepairTraceToContextHandler(_db);
        _movementHandler = new RegisterBqMovementHandler(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task<Tool> SeedToolAsync(string toolId)
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

    private Task<CreateBqRepairTraceOutcome> CreateTraceAsync(string toolId) =>
        _createHandler.HandleAsync(new CreateBqRepairTrace(toolId));

    private Task<AssociateBqRepairTraceToContextOutcome> AssociateAsync(
        int traceId,
        int bqContextId) =>
        _associateHandler.HandleAsync(
            new AssociateBqRepairTraceToContext(traceId, bqContextId));

    // ------------------------------------------------------------------
    // Create
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExistingToolId_CreatesTraceWithNullBqContextId()
    {
        await SeedToolAsync("BQ-TOOL-9");

        var outcome = await CreateTraceAsync("BQ-TOOL-9");

        Assert.Equal(CreateBqRepairTraceResult.Success, outcome.Result);
        Assert.NotNull(outcome.Trace);
        Assert.Equal("BQ-TOOL-9", outcome.Trace!.ToolId);
        Assert.Null(outcome.Trace.BqContextId);

        var persisted = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == outcome.Trace.BqRepairTraceId);
        Assert.Equal("BQ-TOOL-9", persisted.ToolId);
        Assert.Null(persisted.BqContextId);
        Assert.Equal(0, await _db.JobOns.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task UnknownToolId_Refused()
    {
        var outcome = await CreateTraceAsync("BQ-TOOL-UNREGISTERED");

        Assert.Equal(CreateBqRepairTraceResult.ToolNotFound, outcome.Result);
        Assert.Null(outcome.Trace);
        Assert.Equal(0, await _db.BqRepairTraces.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task TwoUnresolvedTraces_ForSameTool_Coexist()
    {
        // A cardinalidade de traces não resolvidos por tool_id está em
        // aberto no blueprint: nada a impede a coexistência.
        await SeedToolAsync("BQ-TOOL-9");

        var first = await CreateTraceAsync("BQ-TOOL-9");
        var second = await CreateTraceAsync("BQ-TOOL-9");

        Assert.Equal(CreateBqRepairTraceResult.Success, first.Result);
        Assert.Equal(CreateBqRepairTraceResult.Success, second.Result);
        Assert.NotEqual(first.Trace!.BqRepairTraceId, second.Trace!.BqRepairTraceId);

        var unresolved = await _db.BqRepairTraces.AsNoTracking()
            .Where(t => t.ToolId == "BQ-TOOL-9")
            .ToListAsync();
        Assert.Equal(2, unresolved.Count);
        Assert.All(unresolved, t => Assert.Null(t.BqContextId));
    }

    // ------------------------------------------------------------------
    // Associate
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExistingTrace_AssociatesToExistingBqContext()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var created = await CreateTraceAsync("BQ-TOOL-9");
        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");

        var outcome = await AssociateAsync(created.Trace!.BqRepairTraceId, bq.Id);

        Assert.Equal(AssociateBqRepairTraceToContextResult.Success, outcome.Result);
        Assert.NotNull(outcome.Trace);
        Assert.Equal(created.Trace.BqRepairTraceId, outcome.Trace!.BqRepairTraceId);
        Assert.Equal("BQ-TOOL-9", outcome.Trace.ToolId);
        Assert.Equal(bq.Id, outcome.Trace.BqContextId);

        var persisted = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == created.Trace.BqRepairTraceId);
        Assert.Equal(bq.Id, persisted.BqContextId);
        Assert.Equal("BQ-TOOL-9", persisted.ToolId);
    }

    [Fact]
    public async Task Association_PreservesTraceIdentityToolIdAndMovements()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var created = await CreateTraceAsync("BQ-TOOL-9");
        var traceId = created.Trace!.BqRepairTraceId;

        // Movimentos pré-produção no trace.
        var saida = await _movementHandler.HandleAsync(
            new RegisterBqMovement(traceId, "saida", 5));
        var entrada = await _movementHandler.HandleAsync(
            new RegisterBqMovement(traceId, "entrada", 7));
        Assert.Equal(RegisterBqMovementResult.Success, saida.Result);
        Assert.Equal(RegisterBqMovementResult.Success, entrada.Result);

        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");
        var outcome = await AssociateAsync(traceId, bq.Id);

        Assert.Equal(AssociateBqRepairTraceToContextResult.Success, outcome.Result);
        Assert.Equal(traceId, outcome.Trace!.BqRepairTraceId);
        Assert.Equal("BQ-TOOL-9", outcome.Trace.ToolId);

        // O mesmo trace (mesma identidade, mesmo tool_id), agora com bq_id;
        // os movimentos existentes permanecem exatamente como estavam.
        var traceAfter = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == traceId);
        Assert.Equal("BQ-TOOL-9", traceAfter.ToolId);
        Assert.Equal(bq.Id, traceAfter.BqContextId);

        var movements = await _db.BqMovements.AsNoTracking()
            .Where(m => m.BqRepairTraceId == traceId)
            .OrderBy(m => m.Id)
            .ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.Equal(saida.Movement!.MovementId, movements[0].Id);
        Assert.Equal(entrada.Movement!.MovementId, movements[1].Id);
        Assert.Equal(BqMovementType.Saida, movements[0].Type);
        Assert.Equal(5, movements[0].Quantity);
        Assert.Null(movements[0].Discrepancy);
        Assert.Equal(BqMovementType.Entrada, movements[1].Type);
        Assert.Equal(7, movements[1].Quantity);
        Assert.Equal(-2, movements[1].Discrepancy);

        // Nenhum trace foi recriado nem copiado.
        Assert.Equal(1, await _db.BqRepairTraces.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task UnknownTrace_Refused()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");

        var outcome = await AssociateAsync(9999, bq.Id);

        Assert.Equal(AssociateBqRepairTraceToContextResult.TraceNotFound, outcome.Result);
        Assert.Null(outcome.Trace);
        Assert.False(await _db.BqRepairTraces.AsNoTracking()
            .AnyAsync(t => t.BqContextId == bq.Id));
    }

    [Fact]
    public async Task UnknownBqContext_Refused()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var created = await CreateTraceAsync("BQ-TOOL-9");

        var outcome = await AssociateAsync(created.Trace!.BqRepairTraceId, 9999);

        Assert.Equal(AssociateBqRepairTraceToContextResult.BqContextNotFound, outcome.Result);
        Assert.Null(outcome.Trace);

        var traceAfter = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == created.Trace.BqRepairTraceId);
        Assert.Null(traceAfter.BqContextId);
    }

    [Fact]
    public async Task BqContextAlreadyTracedByAnotherTrace_RefusedCleanly()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");

        var first = await CreateTraceAsync("BQ-TOOL-9");
        var second = await CreateTraceAsync("BQ-TOOL-9");

        var firstOutcome = await AssociateAsync(first.Trace!.BqRepairTraceId, bq.Id);
        Assert.Equal(AssociateBqRepairTraceToContextResult.Success, firstOutcome.Result);

        // Um bq_id tem um único bq_repair_trace_id: recusa tipada limpa.
        var secondOutcome = await AssociateAsync(second.Trace!.BqRepairTraceId, bq.Id);
        Assert.Equal(AssociateBqRepairTraceToContextResult.BqContextAlreadyTraced, secondOutcome.Result);
        Assert.Null(secondOutcome.Trace);

        var secondAfter = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == second.Trace.BqRepairTraceId);
        Assert.Null(secondAfter.BqContextId);

        var owner = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.BqContextId == bq.Id);
        Assert.Equal(first.Trace.BqRepairTraceId, owner.Id);
    }

    [Fact]
    public async Task Association_DoesNotModifyJobOnOrBqContextData()
    {
        await SeedToolAsync("BQ-TOOL-9");
        var created = await CreateTraceAsync("BQ-TOOL-9");
        var bq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");

        var jobOnId = bq.JobOnId;
        var machineId = (await _db.JobOns.AsNoTracking()
            .SingleAsync(j => j.Id == jobOnId)).MachineId;

        var outcome = await AssociateAsync(created.Trace!.BqRepairTraceId, bq.Id);
        Assert.Equal(AssociateBqRepairTraceToContextResult.Success, outcome.Result);

        var jobOnAfter = await _db.JobOns.AsNoTracking().SingleAsync(j => j.Id == jobOnId);
        Assert.Equal("9389T194", jobOnAfter.Reference);
        Assert.Equal("PN-202609", jobOnAfter.ProductionNumber);
        Assert.Equal(machineId, jobOnAfter.MachineId);

        var contextAfter = await _db.BqContexts.AsNoTracking().SingleAsync(c => c.Id == bq.Id);
        Assert.Equal(jobOnId, contextAfter.JobOnId);
        Assert.Equal("BQ-TOOL-9", contextAfter.ToolId);
    }

    // ------------------------------------------------------------------
    // Derived refusals grounded in current authority
    // ------------------------------------------------------------------

    [Fact]
    public async Task AlreadyAssociatedTrace_ReassociationRefused()
    {
        // Uma produção posterior nunca toma posse de um trace anterior:
        // os retornos tardios ficam no trace de origem.
        await SeedToolAsync("BQ-TOOL-9");
        var created = await CreateTraceAsync("BQ-TOOL-9");
        var firstBq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202609");
        var secondBq = await SeedProductionBqContextAsync("BQ-TOOL-9", "PN-202610");

        var first = await AssociateAsync(created.Trace!.BqRepairTraceId, firstBq.Id);
        Assert.Equal(AssociateBqRepairTraceToContextResult.Success, first.Result);

        var second = await AssociateAsync(created.Trace.BqRepairTraceId, secondBq.Id);
        Assert.Equal(AssociateBqRepairTraceToContextResult.TraceAlreadyAssociated, second.Result);
        Assert.Null(second.Trace);

        var traceAfter = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == created.Trace.BqRepairTraceId);
        Assert.Equal(firstBq.Id, traceAfter.BqContextId);
        Assert.False(await _db.BqRepairTraces.AsNoTracking()
            .AnyAsync(t => t.BqContextId == secondBq.Id));
    }

    [Fact]
    public async Task ToolMismatchBetweenTraceAndBqContext_Refused()
    {
        // A correlação trace ↔ bq_id faz-se pelo mesmo tool_id canónico.
        await SeedToolAsync("BQ-TOOL-9");
        await SeedToolAsync("BQ-TOOL-7");
        var created = await CreateTraceAsync("BQ-TOOL-9");
        var otherToolBq = await SeedProductionBqContextAsync("BQ-TOOL-7", "PN-202609");

        var outcome = await AssociateAsync(created.Trace!.BqRepairTraceId, otherToolBq.Id);

        Assert.Equal(
            AssociateBqRepairTraceToContextResult.TraceToolDoesNotMatchBqContextTool,
            outcome.Result);
        Assert.Null(outcome.Trace);

        var traceAfter = await _db.BqRepairTraces.AsNoTracking()
            .SingleAsync(t => t.Id == created.Trace.BqRepairTraceId);
        Assert.Null(traceAfter.BqContextId);
        Assert.False(await _db.BqRepairTraces.AsNoTracking()
            .AnyAsync(t => t.BqContextId == otherToolBq.Id));
    }
}
