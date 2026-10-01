using System.Net;
using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Identity;
using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DMO.Alpha.Web.Tests;

/// <summary>
/// Prova o slice vertical nativo do Registo de Boquilhas: o surface Razor
/// existente recebe um bq_repair_trace_id canónico (?view=registo&amp;trace_id=N),
/// renderiza factos persistidos a partir de dados do servidor, liga o POST de
/// movimento ao IRegisterBqMovementHandler (discrepância computada pelo
/// backend) e volta ao mesmo contexto canónico por redirect. O contexto
/// nativo usa a seam de conta existente: sem sessão autenticada não há
/// leitura nem mutação nativa. A memória/mock do browser não é fonte de
/// verdade deste caminho: os valores semeados existem apenas na persistência.
/// </summary>
public class BoquilhasRegistoPageTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string PagePath = "/31_BOQUILHAS_01_VISUAL_AUTHORITY_boquilhas.html";

    private readonly CustomWebApplicationFactory _factory;

    public BoquilhasRegistoPageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
        ResetDomainState();
    }

    private void ResetDomainState()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        db.BqMovements.RemoveRange(db.BqMovements);
        db.BqRepairTraces.RemoveRange(db.BqRepairTraces);
        db.BqContexts.RemoveRange(db.BqContexts);
        db.CmContexts.RemoveRange(db.CmContexts);
        db.MfContexts.RemoveRange(db.MfContexts);
        db.JobOns.RemoveRange(db.JobOns);
        db.Machines.RemoveRange(db.Machines);
        db.Tools.RemoveRange(db.Tools);
        db.SaveChanges();
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private void SeedUser(string operatorId, string name, string providerUserId, string password)
    {
        var generatedEmail = $"{operatorId}@dmo.local";
        _factory.AuthProvider.AddAccount(generatedEmail, providerUserId, password, generatedEmail);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            OperatorId = operatorId,
            Name = name,
            ProviderUserId = providerUserId
        });
        db.SaveChanges();
    }

    private async Task<HttpClient> LoginClientAsync(string identifier, string password)
    {
        var client = CreateClient();
        var token = await GetVerificationTokenAsync(client, "/login");
        var response = await client.PostAsync("/login", HtmlFormHelpers.BuildFormPost(
            ("identifier", identifier),
            ("password", password),
            ("returnUrl", "/"),
            ("__RequestVerificationToken", token)));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    private async Task<string> GetVerificationTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return HtmlFormHelpers.ExtractRequestVerificationToken(html)
            ?? throw new InvalidOperationException($"No antiforgery token found on {path}.");
    }

    private async Task<Tool> SeedToolAsync(string toolId, string reference, string lot)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        var tool = new Tool { ToolId = toolId, Type = ToolType.Bq, Reference = reference, Lot = lot };
        db.Tools.Add(tool);
        await db.SaveChangesAsync();
        return tool;
    }

    private async Task<BqRepairTrace> SeedTraceAsync(string toolId, int? bqContextId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        var trace = new BqRepairTrace { ToolId = toolId, BqContextId = bqContextId };
        db.BqRepairTraces.Add(trace);
        await db.SaveChangesAsync();
        return trace;
    }

    private async Task<BqContext> SeedProductionAsync(
        string toolId,
        string reference,
        string productionNumber,
        string machineCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        var machine = new Machine { Code = machineCode };
        var jobOn = new JobOn { Reference = reference, ProductionNumber = productionNumber, Machine = machine };
        var bq = new BqContext { JobOn = jobOn, ToolId = toolId };
        db.JobOns.Add(jobOn);
        db.BqContexts.Add(bq);
        await db.SaveChangesAsync();
        return bq;
    }

    private async Task SeedMovementAsync(int traceId, string movementType, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRegisterBqMovementHandler>();
        var outcome = await handler.HandleAsync(new RegisterBqMovement(traceId, movementType, quantity));
        Assert.Equal(RegisterBqMovementResult.Success, outcome.Result);
    }

    private async Task<List<BqMovement>> ReadMovementsAsync(int traceId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        return await db.BqMovements.AsNoTracking()
            .Where(m => m.BqRepairTraceId == traceId)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    private string TraceUrl(int traceId) => $"{PagePath}?view=registo&trace_id={traceId}";

    private async Task<string> GetPageHtmlAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<HttpResponseMessage> PostMovementAsync(
        HttpClient client,
        string tokenPath,
        int traceId,
        string movementType,
        int quantity)
    {
        var token = await GetVerificationTokenAsync(client, tokenPath);
        return await client.PostAsync(
            $"{PagePath}?handler=RegisterMovement",
            HtmlFormHelpers.BuildFormPost(
                ("trace_id", traceId.ToString()),
                ("movement_type", movementType),
                ("quantity", quantity.ToString()),
                ("__RequestVerificationToken", token)));
    }

    // ------------------------------------------------------------------
    // Canonical page context + read model
    // ------------------------------------------------------------------

    [Fact]
    public async Task CanonicalTraceId_LoadsPersistedTrace()
    {
        await SeedToolAsync("BQ-NATIVE-1", "RGT997", "77");
        var trace = await SeedTraceAsync("BQ-NATIVE-1");
        SeedUser("4100", "Registo Native", "provider-registo-1", "RegistoPass1!");
        using var client = await LoginClientAsync("4100", "RegistoPass1!");

        var html = await GetPageHtmlAsync(client, TraceUrl(trace.Id));

        // Read model resolvido por bq_repair_trace_id canónico, sem bq_id.
        Assert.Contains("id=\"nativeRegistoTrace\"", html);
        Assert.Contains($"bq_repair_trace_id {trace.Id}", html);
        Assert.Contains("tool_id BQ-NATIVE-1", html);
        Assert.Contains("bq_id por associar", html);
        Assert.Contains("Produção por associar", html);
        // Trace vazio: derivados fechados no estado zero.
        Assert.Contains("Sem movimentos registados neste trace.", html);
    }

    [Fact]
    public async Task PersistedToolAndContextFacts_RenderFromServerData()
    {
        await SeedToolAsync("BQ-NATIVE-2", "RGT998", "78");
        var bq = await SeedProductionAsync("BQ-NATIVE-2", "ZQREF-99", "PN-550011", "B7");
        var trace = await SeedTraceAsync("BQ-NATIVE-2", bqContextId: bq.Id);
        SeedUser("4101", "Registo Native", "provider-registo-2", "RegistoPass2!");
        using var client = await LoginClientAsync("4101", "RegistoPass2!");

        var html = await GetPageHtmlAsync(client, TraceUrl(trace.Id));

        // Estes valores existem apenas na persistência semeada — nada de
        // fixtures de browser os fornece.
        Assert.Contains($"bq_id {bq.Id}", html);
        Assert.Contains("ZQREF-99 · Produção PN-550011 · B7 — contexto Job On, apenas leitura", html);
    }

    // ------------------------------------------------------------------
    // Movement POST through IRegisterBqMovementHandler
    // ------------------------------------------------------------------

    [Fact]
    public async Task PostSaida_PersistsThroughHandler()
    {
        await SeedToolAsync("BQ-NATIVE-3", "RGT999", "79");
        var trace = await SeedTraceAsync("BQ-NATIVE-3");
        SeedUser("4102", "Registo Native", "provider-registo-3", "RegistoPass3!");
        using var client = await LoginClientAsync("4102", "RegistoPass3!");

        var response = await PostMovementAsync(client, TraceUrl(trace.Id), trace.Id, "saida", 5);

        // PRG: volta ao mesmo contexto canónico do trace.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? string.Empty;
        Assert.Contains("view=registo", location);
        Assert.Contains($"trace_id={trace.Id}", location);

        var movements = await ReadMovementsAsync(trace.Id);
        var persisted = Assert.Single(movements);
        Assert.Equal(BqMovementType.Saida, persisted.Type);
        Assert.Equal(5, persisted.Quantity);
        Assert.Null(persisted.Discrepancy);

        // Re-render nativo a partir da persistência (sem JS/browser memory).
        var html = await GetPageHtmlAsync(client, location);
        Assert.Contains($"<tr><td>{persisted.Id}</td><td>saida</td><td>5</td><td>—</td></tr>", html);
        Assert.Contains("<span>Movimentos persistidos</span><strong>1</strong>", html);
    }

    [Fact]
    public async Task PostEntrada_ServerComputedDiscrepancyReflected()
    {
        await SeedToolAsync("BQ-NATIVE-4", "RGT994", "80");
        var trace = await SeedTraceAsync("BQ-NATIVE-4");
        await SeedMovementAsync(trace.Id, "saida", 5);
        SeedUser("4103", "Registo Native", "provider-registo-4", "RegistoPass4!");
        using var client = await LoginClientAsync("4103", "RegistoPass4!");

        var response = await PostMovementAsync(client, TraceUrl(trace.Id), trace.Id, "entrada", 7);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var movements = await ReadMovementsAsync(trace.Id);
        Assert.Equal(2, movements.Count);
        var entrada = movements[1];
        Assert.Equal(BqMovementType.Entrada, entrada.Type);
        Assert.Equal(7, entrada.Quantity);
        // Discrepância computada pelo backend; o browser nunca a envia.
        Assert.Equal(-2, entrada.Discrepancy);

        var html = await GetPageHtmlAsync(client, response.Headers.Location!.OriginalString);
        Assert.Contains($"<tr><td>{entrada.Id}</td><td>entrada</td><td>7</td><td>-2</td></tr>", html);
        Assert.Contains("<span>Discrepância acumulada</span><strong>-2</strong>", html);
        Assert.Contains("<span>Quantidade legitimamente fora</span><strong>0</strong>", html);
    }

    [Fact]
    public async Task PostEntradaSemReparacao_PersistsWithClosedSemantics()
    {
        await SeedToolAsync("BQ-NATIVE-5", "RGT995", "81");
        var trace = await SeedTraceAsync("BQ-NATIVE-5");
        await SeedMovementAsync(trace.Id, "saida", 5);
        SeedUser("4104", "Registo Native", "provider-registo-5", "RegistoPass5!");
        using var client = await LoginClientAsync("4104", "RegistoPass5!");

        var response = await PostMovementAsync(
            client, TraceUrl(trace.Id), trace.Id, "entrada_sem_reparacao", 3);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var movements = await ReadMovementsAsync(trace.Id);
        Assert.Equal(2, movements.Count);
        var semReparacao = movements[1];
        // Token distinto persistido; contabilidade idêntica à entrada:
        // 3 explicados sobre 5 fora, sem discrepância.
        Assert.Equal(BqMovementType.EntradaSemReparacao, semReparacao.Type);
        Assert.Equal(3, semReparacao.Quantity);
        Assert.Null(semReparacao.Discrepancy);

        var html = await GetPageHtmlAsync(client, response.Headers.Location!.OriginalString);
        Assert.Contains(
            $"<tr><td>{semReparacao.Id}</td><td>entrada_sem_reparacao</td><td>3</td><td>—</td></tr>",
            html);
        Assert.Contains("<span>Quantidade legitimamente fora</span><strong>2</strong>", html);
    }

    // ------------------------------------------------------------------
    // Clean refusals
    // ------------------------------------------------------------------

    [Fact]
    public async Task UnknownTrace_RefusedCleanly()
    {
        await SeedToolAsync("BQ-NATIVE-6", "RGT996", "82");
        SeedUser("4105", "Registo Native", "provider-registo-6", "RegistoPass6!");
        using var client = await LoginClientAsync("4105", "RegistoPass6!");

        var html = await GetPageHtmlAsync(client, TraceUrl(99999));
        Assert.Contains("Registo não encontrado", html);
        Assert.Contains("O bq_repair_trace_id pedido não existe.", html);

        // POST a trace desconhecido: recusa tipada por query, nada persistido.
        var response = await PostMovementAsync(client, "/login", 99999, "saida", 5);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? string.Empty;
        Assert.Contains("movement_refusal=TraceNotFound", location);

        var refusalHtml = await GetPageHtmlAsync(client, location);
        Assert.Contains("O registo indicado não existe. Nenhum movimento foi registado.", refusalHtml);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        Assert.Equal(0, await db.BqMovements.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task UnknownMovementType_RefusedViaTypedRefusal()
    {
        await SeedToolAsync("BQ-NATIVE-7", "RGT993", "83");
        var trace = await SeedTraceAsync("BQ-NATIVE-7");
        SeedUser("4106", "Registo Native", "provider-registo-7", "RegistoPass7!");
        using var client = await LoginClientAsync("4106", "RegistoPass7!");

        // "Irreparável" não é token canónico e não é mapeado.
        var response = await PostMovementAsync(client, TraceUrl(trace.Id), trace.Id, "Irreparável", 4);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? string.Empty;
        Assert.Contains("movement_refusal=UnknownMovementType", location);

        var html = await GetPageHtmlAsync(client, location);
        Assert.Contains("Tipo de movimento desconhecido. Use saida, entrada ou entrada_sem_reparacao.", html);
        Assert.Empty(await ReadMovementsAsync(trace.Id));
    }

    // ------------------------------------------------------------------
    // Immutability of Job On / BqContext
    // ------------------------------------------------------------------

    [Fact]
    public async Task PostMovement_DoesNotModifyJobOnOrBqContext()
    {
        await SeedToolAsync("BQ-NATIVE-8", "RGT992", "84");
        var bq = await SeedProductionAsync("BQ-NATIVE-8", "ZQREF-98", "PN-550012", "B8");
        var trace = await SeedTraceAsync("BQ-NATIVE-8", bqContextId: bq.Id);
        SeedUser("4107", "Registo Native", "provider-registo-8", "RegistoPass8!");
        using var client = await LoginClientAsync("4107", "RegistoPass8!");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
            var jobOnBefore = await db.JobOns.AsNoTracking().SingleAsync(j => j.Id == bq.JobOnId);
            Assert.Equal("ZQREF-98", jobOnBefore.Reference);
            Assert.Equal("PN-550012", jobOnBefore.ProductionNumber);
        }

        var response = await PostMovementAsync(client, TraceUrl(trace.Id), trace.Id, "entrada", 2);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
            var jobOnAfter = await db.JobOns.AsNoTracking().SingleAsync(j => j.Id == bq.JobOnId);
            Assert.Equal("ZQREF-98", jobOnAfter.Reference);
            Assert.Equal("PN-550012", jobOnAfter.ProductionNumber);
            Assert.NotEqual(0, jobOnAfter.MachineId);

            var contextAfter = await db.BqContexts.AsNoTracking().SingleAsync(c => c.Id == bq.Id);
            Assert.Equal(bq.JobOnId, contextAfter.JobOnId);
            Assert.Equal("BQ-NATIVE-8", contextAfter.ToolId);

            var traceAfter = await db.BqRepairTraces.AsNoTracking().SingleAsync(t => t.Id == trace.Id);
            Assert.Equal(bq.Id, traceAfter.BqContextId);
            Assert.Equal("BQ-NATIVE-8", traceAfter.ToolId);

            Assert.Single(await db.BqMovements.AsNoTracking()
                .Where(m => m.BqRepairTraceId == trace.Id).ToListAsync());
        }
    }

    // ------------------------------------------------------------------
    // Server persistence is the source of truth (not browser memory)
    // ------------------------------------------------------------------

    [Fact]
    public async Task NativePath_ServerDataIsSourceOfTruth()
    {
        // Referência exclusiva da persistência: nenhum fixture/mock do
        // browser a contém; se renderiza, veio do servidor.
        await SeedToolAsync("BQ-NATIVE-9", "RGT-UNIQ-5501", "85");
        var trace = await SeedTraceAsync("BQ-NATIVE-9");
        SeedUser("4108", "Registo Native", "provider-registo-9", "RegistoPass9!");
        using var client = await LoginClientAsync("4108", "RegistoPass9!");

        var html = await GetPageHtmlAsync(client, TraceUrl(trace.Id));
        Assert.Contains("RGT-UNIQ-5501", html);

        var response = await PostMovementAsync(client, TraceUrl(trace.Id), trace.Id, "saida", 9);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var movements = await ReadMovementsAsync(trace.Id);
        var persisted = Assert.Single(movements);

        // O re-render após o POST vem do GET seguinte ao redirect: apenas
        // dados persistidos, sem qualquer execução de JavaScript.
        var reloaded = await GetPageHtmlAsync(client, response.Headers.Location!.OriginalString);
        Assert.Contains($"<tr><td>{persisted.Id}</td><td>saida</td><td>9</td><td>—</td></tr>", reloaded);
        Assert.Contains("<span>Quantidade legitimamente fora</span><strong>9</strong>", reloaded);
    }

    // ------------------------------------------------------------------
    // Existing account seam gates the native read and the native mutation
    // ------------------------------------------------------------------

    [Fact]
    public async Task Anonymous_NativeTraceRead_RedirectedToLogin()
    {
        await SeedToolAsync("BQ-NATIVE-10", "RGT991", "86");
        var trace = await SeedTraceAsync("BQ-NATIVE-10");

        using var client = CreateClient();
        var response = await client.GetAsync(TraceUrl(trace.Id));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/login", location);
        Assert.Contains("ReturnUrl=", location);
    }

    [Fact]
    public async Task Anonymous_MovementPost_RedirectedToLogin_NothingPersisted()
    {
        await SeedToolAsync("BQ-NATIVE-11", "RGT990", "87");
        var trace = await SeedTraceAsync("BQ-NATIVE-11");

        using var client = CreateClient();
        var token = await GetVerificationTokenAsync(client, "/login");
        var response = await client.PostAsync(
            $"{PagePath}?handler=RegisterMovement",
            HtmlFormHelpers.BuildFormPost(
                ("trace_id", trace.Id.ToString()),
                ("movement_type", "saida"),
                ("quantity", "5"),
                ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location?.ToString() ?? string.Empty);
        Assert.Empty(await ReadMovementsAsync(trace.Id));
    }
}
