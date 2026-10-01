using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Boquilhas;

/// <summary>
/// Prova o write path de registo de movimento de Boquilhas: escritas com
/// sucesso (saida sem discrepância; entrada e entrada_sem_reparacao com a
/// regra idêntica de matched/unmatched — entrada_sem_reparacao reduz o
/// outstanding exatamente como entrada e o token distinto é preservado) e
/// recusas tipadas (trace inexistente, Tool não registada no registo
/// canónico, tipo não autorizado). O registo não depende do Tool.Type e
/// nenhuma escrita altera Job On, BqContext ou o trace.
/// </summary>
public sealed class RegisterBqMovementHandlerTests : IDisposable
{
    private readonly DmoDbContext _db;
    private readonly RegisterBqMovementHandler _handler;

    public RegisterBqMovementHandlerTests()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new DmoDbContext(options);
        _db.Database.EnsureCreated();
        _handler = new RegisterBqMovementHandler(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task<Tool> SeedToolAsync(string toolId, ToolType type)
    {
        var tool = new Tool
        {
            ToolId = toolId,
            Type = type,
            Reference = "9389T194",
            Lot = "12"
        };
        _db.Tools.Add(tool);
        await _db.SaveChangesAsync();
        return tool;
    }

    private async Task<BqRepairTrace> SeedTraceAsync(string toolId)
    {
        var trace = new BqRepairTrace { ToolId = toolId };
        _db.BqRepairTraces.Add(trace);
        await _db.SaveChangesAsync();
        return trace;
    }

    private Task<RegisterBqMovementOutcome> RegisterAsync(
        BqRepairTrace trace,
        string type,
        int quantity) =>
        _handler.HandleAsync(new RegisterBqMovement(trace.Id, type, quantity));

    [Fact]
    public async Task Saida_RegistersMovementWithoutDiscrepancy()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        var outcome = await RegisterAsync(trace, "saida", 5);

        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.NotNull(outcome.Movement);
        Assert.Equal(trace.Id, outcome.Movement!.BqRepairTraceId);
        Assert.Equal("saida", outcome.Movement.MovementType);
        Assert.Equal(5, outcome.Movement.Quantity);
        Assert.Null(outcome.Movement.Discrepancy);

        var persisted = await _db.BqMovements.AsNoTracking()
            .SingleAsync(m => m.Id == outcome.Movement.MovementId);
        Assert.Equal(trace.Id, persisted.BqRepairTraceId);
        Assert.Equal(BqMovementType.Saida, persisted.Type);
        Assert.Equal(5, persisted.Quantity);
        Assert.Null(persisted.Discrepancy);
    }

    [Fact]
    public async Task Entrada_FullyMatched_HasNoDiscrepancy()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        var outcome = await RegisterAsync(trace, "entrada", 5);

        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal(5, outcome.Movement!.Quantity);
        Assert.Null(outcome.Movement.Discrepancy);
    }

    [Fact]
    public async Task Entrada_WithUnexplainedQuantity_RecordsFullQuantityAndNegativeDiscrepancy()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        var outcome = await RegisterAsync(trace, "entrada", 7);

        // A Entrada observada é registada na totalidade (7, não truncada
        // para 5) e produz a discrepância -2 (movimento real aceite).
        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal(7, outcome.Movement!.Quantity);
        Assert.Equal(-2, outcome.Movement.Discrepancy);
    }

    [Fact]
    public async Task Entrada_WithoutAnySaida_IsAcceptedInFull()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        // Proibido rejeitar uma Entrada por exceder a quantidade explicável.
        var outcome = await RegisterAsync(trace, "entrada", 3);

        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal(3, outcome.Movement!.Quantity);
        Assert.Equal(-3, outcome.Movement.Discrepancy);
    }

    [Fact]
    public async Task Entrada_FollowsTraceHistorySequence()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        var first = await RegisterAsync(trace, "entrada", 7);
        var second = await RegisterAsync(trace, "entrada", 5);

        // Cada Entrada produz a sua própria discrepância histórica contra o
        // estado precedente do trace; a segunda não compensa a primeira.
        Assert.Equal(-2, first.Movement!.Discrepancy);
        Assert.Equal(-5, second.Movement!.Discrepancy);

        var movements = await _db.BqMovements.AsNoTracking()
            .Where(m => m.BqRepairTraceId == trace.Id)
            .OrderBy(m => m.Id)
            .ToListAsync();
        Assert.Equal(3, movements.Count);
        Assert.Equal(3, movements.Select(m => m.Id).Distinct().Count());
    }

    [Fact]
    public async Task EntradaSemReparacao_AfterSaida_ReducesOutstanding()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        var retorno = await RegisterAsync(trace, "entrada_sem_reparacao", 3);

        // Reduz o outstanding exatamente como entrada: 3 explicados, sem
        // discrepância.
        Assert.Equal(RegisterBqMovementResult.Success, retorno.Result);
        Assert.Equal("entrada_sem_reparacao", retorno.Movement!.MovementType);
        Assert.Equal(3, retorno.Movement.Quantity);
        Assert.Null(retorno.Movement.Discrepancy);

        // Sobram 2 legitimamente fora: uma Entrada de 2 é totalmente
        // explicável.
        var later = await RegisterAsync(trace, "entrada", 2);
        Assert.Equal(RegisterBqMovementResult.Success, later.Result);
        Assert.Equal(2, later.Movement!.Quantity);
        Assert.Null(later.Movement.Discrepancy);
    }

    [Fact]
    public async Task EntradaSemReparacao_Excess_RecordsFullQuantityAndNegativeDiscrepancy()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        var outcome = await RegisterAsync(trace, "entrada_sem_reparacao", 7);

        // Quantidade observada na totalidade (7) e a mesma discrepância
        // negativa de entrada (matched 5, unmatched 2).
        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal(7, outcome.Movement!.Quantity);
        Assert.Equal(-2, outcome.Movement.Discrepancy);
    }

    [Fact]
    public async Task Entrada_AfterEntradaSemReparacao_UsesRemainingOutstanding()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        await RegisterAsync(trace, "saida", 5);
        await RegisterAsync(trace, "entrada_sem_reparacao", 3);

        // Outstanding restante: 2. Entrada de 3 → matched 2, unmatched 1.
        var outcome = await RegisterAsync(trace, "entrada", 3);

        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal(3, outcome.Movement!.Quantity);
        Assert.Equal(-1, outcome.Movement.Discrepancy);
    }

    [Fact]
    public async Task MovementRegistration_DoesNotDependOnToolType()
    {
        // O trace já transporta o tool_id relevante; o Tool.Type não é um
        // gate comportamental — um trace ancorado a uma Tool CM regista
        // movimentos normalmente.
        await SeedToolAsync("CM-TOOL-1", ToolType.Cm);
        var trace = await SeedTraceAsync("CM-TOOL-1");

        var outcome = await RegisterAsync(trace, "saida", 5);

        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
        Assert.Equal("saida", outcome.Movement!.MovementType);

        var persisted = await _db.BqMovements.AsNoTracking()
            .SingleAsync(m => m.Id == outcome.Movement.MovementId);
        Assert.Equal(BqMovementType.Saida, persisted.Type);
        Assert.Equal(5, persisted.Quantity);
    }

    [Fact]
    public async Task DistinctMovementToken_PreservedInHistory()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        var saida = await RegisterAsync(trace, "saida", 5);
        var entrada = await RegisterAsync(trace, "entrada", 5);
        var semReparacao = await RegisterAsync(trace, "entrada_sem_reparacao", 2);

        Assert.Equal(RegisterBqMovementResult.Success, saida.Result);
        Assert.Equal(RegisterBqMovementResult.Success, entrada.Result);
        Assert.Equal(RegisterBqMovementResult.Success, semReparacao.Result);

        // O token de entrada_sem_reparacao permanece distinto no histórico
        // persistido — não é colapsado em entrada.
        var persisted = await _db.BqMovements.AsNoTracking()
            .Where(m => m.BqRepairTraceId == trace.Id)
            .OrderBy(m => m.Id)
            .ToListAsync();

        Assert.Equal(
            new[] { BqMovementType.Saida, BqMovementType.Entrada, BqMovementType.EntradaSemReparacao },
            persisted.Select(m => m.Type).ToArray());
        Assert.Equal("entrada_sem_reparacao", semReparacao.Movement!.MovementType);
        Assert.Equal(BqMovementType.EntradaSemReparacao,
            persisted.Single(m => m.Id == semReparacao.Movement.MovementId).Type);
    }

    [Fact]
    public async Task UnknownTrace_RefusedTraceNotFound()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        await SeedTraceAsync("BQ-TOOL-9");

        var outcome = await _handler.HandleAsync(new RegisterBqMovement(9999, "saida", 5));

        Assert.Equal(RegisterBqMovementResult.TraceNotFound, outcome.Result);
        Assert.Null(outcome.Movement);
        Assert.Equal(0, await _db.BqMovements.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task TraceToolMissingFromRegistry_Refused()
    {
        var trace = await SeedTraceAsync("BQ-TOOL-UNREGISTERED");

        var outcome = await RegisterAsync(trace, "saida", 5);

        Assert.Equal(RegisterBqMovementResult.TraceToolNotRegistered, outcome.Result);
        Assert.Null(outcome.Movement);
        Assert.Equal(0, await _db.BqMovements.AsNoTracking().CountAsync());
    }

    [Theory]
    [InlineData("Irreparável")]
    [InlineData("SAIDA")]
    [InlineData("Saída")]
    [InlineData("")]
    public async Task UnauthorizedTypeTokens_Refused(string token)
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);
        var trace = await SeedTraceAsync("BQ-TOOL-9");

        var outcome = await RegisterAsync(trace, token, 5);

        Assert.Equal(RegisterBqMovementResult.UnknownMovementType, outcome.Result);
        Assert.Null(outcome.Movement);
        Assert.Equal(0, await _db.BqMovements.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task RegisteringMovements_DoesNotModifyJobOnBqContextOrTrace()
    {
        await SeedToolAsync("BQ-TOOL-9", ToolType.Bq);

        var machine = new Machine { Code = "B1" };
        var jobOn = new JobOn
        {
            Reference = "9389T194",
            ProductionNumber = "PN-202609",
            Machine = machine
        };
        var bq = new BqContext { JobOn = jobOn, ToolId = "BQ-TOOL-9" };
        _db.JobOns.Add(jobOn);
        _db.BqContexts.Add(bq);

        var trace = new BqRepairTrace { ToolId = "BQ-TOOL-9", BqContextId = bq.Id };
        _db.BqRepairTraces.Add(trace);
        await _db.SaveChangesAsync();

        var jobOnId = jobOn.Id;
        var machineId = jobOn.MachineId;

        // Registar movimentos (pré e pós-produção irrelevante: as datas de
        // produção não controlam o registo) não altera nada no Job On, no
        // BqContext ou no trace.
        await RegisterAsync(trace, "saida", 5);
        await RegisterAsync(trace, "entrada_sem_reparacao", 7);

        var jobOnAfter = await _db.JobOns.AsNoTracking().SingleAsync(j => j.Id == jobOnId);
        Assert.Equal("9389T194", jobOnAfter.Reference);
        Assert.Equal("PN-202609", jobOnAfter.ProductionNumber);
        Assert.Equal(machineId, jobOnAfter.MachineId);

        var contextAfter = await _db.BqContexts.AsNoTracking().SingleAsync(c => c.Id == bq.Id);
        Assert.Equal(jobOnId, contextAfter.JobOnId);
        Assert.Equal("BQ-TOOL-9", contextAfter.ToolId);

        var traceAfter = await _db.BqRepairTraces.AsNoTracking().SingleAsync(t => t.Id == trace.Id);
        Assert.Equal("BQ-TOOL-9", traceAfter.ToolId);
        Assert.Equal(bq.Id, traceAfter.BqContextId);

        var movements = await _db.BqMovements.AsNoTracking()
            .Where(m => m.BqRepairTraceId == trace.Id)
            .OrderBy(m => m.Id)
            .ToListAsync();
        Assert.Equal(2, movements.Count);
        Assert.Equal(2, movements.Select(m => m.Id).Distinct().Count());
    }
}
