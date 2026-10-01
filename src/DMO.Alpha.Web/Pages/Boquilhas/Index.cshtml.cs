using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Web.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMO.Alpha.Web.Pages.Boquilhas;

/// <summary>
/// Surface Boquilhas. A página continua a servir o protótipo demo quando
/// nenhum trace_id canónico é apresentado. Quando o query transporta um
/// bq_repair_trace_id (?view=registo&amp;trace_id=N), a página resolve o
/// read model nativo do Registo a partir da persistência canónica e expõe
/// o POST de movimento ligado ao IRegisterBqMovementHandler. O contexto
/// nativo usa a seam de conta existente (sessão autenticada por cookie):
/// leitura e mutação nativas exigem conta autenticada; o protótipo demo
/// permanece como está.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly IBoquilhasRegistoQuery _registoQuery;
    private readonly IRegisterBqMovementHandler _movementHandler;

    public IndexModel(
        IBoquilhasRegistoQuery registoQuery,
        IRegisterBqMovementHandler movementHandler)
    {
        _registoQuery = registoQuery;
        _movementHandler = movementHandler;
    }

    /// <summary>
    /// Read model nativo do trace canónico pedido; null quando não há
    /// contexto nativo (protótipo demo) ou quando o trace não existe.
    /// </summary>
    public BoquilhasRegistoTrace? RegistoTrace { get; private set; }

    /// <summary>
    /// True quando foi pedido um trace_id canónico que não existe: recusa
    /// limpa e tipada, sem inventar registo nem estado.
    /// </summary>
    public bool TraceRequestedNotFound { get; private set; }

    /// <summary>
    /// Recusa tipada do último POST de movimento, transportada por query
    /// com o vocabulário fechado do enum (nunca texto livre).
    /// </summary>
    public RegisterBqMovementResult? MovementRefusal { get; private set; }

    /// <summary>True quando o contexto nativo está ativo (mock bypassed).</summary>
    public bool NativeRegistoActive => RegistoTrace is not null || TraceRequestedNotFound;

    /// <summary>Mensagem da recusa tipada, apenas apresentação.</summary>
    public string MovementRefusalMessage => MovementRefusal switch
    {
        RegisterBqMovementResult.TraceNotFound =>
            "O registo indicado não existe. Nenhum movimento foi registado.",
        RegisterBqMovementResult.TraceToolNotRegistered =>
            "A ferramenta do registo não está registada no Ferramentas. Nenhum movimento foi registado.",
        RegisterBqMovementResult.UnknownMovementType =>
            "Tipo de movimento desconhecido. Use saida, entrada ou entrada_sem_reparacao. Nenhum movimento foi registado.",
        _ => string.Empty
    };

    public async Task<IActionResult> OnGetAsync(
        [FromQuery(Name = "trace_id")] int? traceId,
        [FromQuery(Name = "movement_refusal")] string? movementRefusal)
    {
        // These are the prototype's existing module-local panel declarations.
        // Only ?view=registo is currently handled by beta-adapter.js. Do not
        // invent deep-link behavior for the other panels during extraction.
        var functions = new FunctionPresentation[]
        {
            new("registo", "Registo", null, "registo", true, true, true),
            new("boquilhas", "Boquilhas", null, "boquilhas", false, true, true),
            new("historico", "Histórico", null, "historico", false, true, true),
            new("definicoes", "Definições", null, "definicoes", false, true, true, "settings-tab")
        };
        ViewData["Shell"] = new ShellPresentation(
            new("boquilhas", "Boquilhas"), "ModuleFunctions", "registo", functions,
            // Planeamento keeps its shell slot visible but has no destination
            // yet: the tab renders unavailable instead of pointing at an
            // unconverted reference page. The future route is not decided here.
            new("Planeamento", string.Empty, false, true, false),
            new("João Silva", "Responsável · Metrologia", "12_LOGIN_01_VISUAL_AUTHORITY_login.html"),
            new Dictionary<string, ActionAvailability>(),
            // The machine side panel is a shell component and is open on this destination.
            Sidepanel: new SidepanelPresentation(ShellSidepanelKind.Boquilhas, Visible: true));
        // Demo session values live only in sessionStorage. The unchanged demo
        // authorization layer binds its resolved account after loading. There
        // native session/capability integration remains outside this proof.
        ViewData["Title"] = "Registo de Boquilhas — Responsável";
        ViewData["BetaPage"] = "boquilhas";

        // Recusa tipada do POST anterior, transportada por query (PRG).
        if (movementRefusal is not null)
        {
            if (Enum.TryParse(movementRefusal, out RegisterBqMovementResult parsed) &&
                parsed != RegisterBqMovementResult.Success)
            {
                MovementRefusal = parsed;
            }
        }

        if (traceId is int canonicalId)
        {
            // Contexto nativo: a seam de conta existente é exigida. Sem sessão
            // autenticada, a leitura nativa é recusada (Challenge → /login).
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            // O trace_id é o transporte de identidade canónico — nunca
            // objetos de trace em JavaScript.
            RegistoTrace = await _registoQuery.FindTraceAsync(canonicalId);
            if (RegistoTrace is null)
            {
                TraceRequestedNotFound = true;
                if (MovementRefusal is null)
                {
                    MovementRefusal = RegisterBqMovementResult.TraceNotFound;
                }
            }
        }

        return Page();
    }

    /// <summary>
    /// POST nativo do Registo: o browser submete apenas o trace_id canónico,
    /// o token canónico e a quantidade observada. A discrepância nunca é
    /// enviada pelo cliente — o backend é a fonte autoritativa. Sucesso
    /// volta ao mesmo contexto canónico por redirect (PRG); a recusa é
    /// tipada e tipada volta por query. Nada é escrito no Job On nem no
    /// BqContext.
    /// </summary>
    public async Task<IActionResult> OnPostRegisterMovementAsync(
        [FromForm(Name = "trace_id")] int traceId,
        [FromForm(Name = "movement_type")] string? movementType,
        [FromForm(Name = "quantity")] int? quantity)
    {
        // A mutação nativa exige a seam de conta existente: sem sessão
        // autenticada nada é escrito.
        if (User?.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var outcome = await _movementHandler.HandleAsync(
            new RegisterBqMovement(traceId, movementType ?? string.Empty, quantity ?? 0),
            HttpContext.RequestAborted);

        if (outcome.Result == RegisterBqMovementResult.Success)
        {
            // Permanece no mesmo contexto canónico do trace e re-carrega o
            // estado persistido a partir do servidor (a memória do browser
            // não é fonte de verdade).
            return RedirectToPage("/Boquilhas/Index", new { view = "registo", trace_id = traceId });
        }

        return RedirectToPage("/Boquilhas/Index", new
        {
            view = "registo",
            trace_id = traceId,
            movement_refusal = outcome.Result.ToString()
        });
    }
}
