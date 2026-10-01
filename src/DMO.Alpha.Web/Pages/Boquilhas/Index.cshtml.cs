using Microsoft.AspNetCore.Mvc.RazorPages;
using DMO.Alpha.Web.Presentation;

namespace DMO.Alpha.Web.Pages.Boquilhas;

public sealed class IndexModel : PageModel
{
    public void OnGet()
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
            new("Planeamento", "20_JOB_ON_01_VISUAL_AUTHORITY_job-on.html", false, true, true),
            new("João Silva", "Responsável · Metrologia", "12_LOGIN_01_VISUAL_AUTHORITY_login.html"),
            new Dictionary<string, ActionAvailability>());
        // Demo session values live only in sessionStorage. The unchanged demo
        // authorization layer binds its resolved account after loading. There
        // native session/capability integration remains outside this proof.
        ViewData["Title"] = "Registo de Boquilhas — Responsável";
        ViewData["BetaPage"] = "boquilhas";
    }
}
