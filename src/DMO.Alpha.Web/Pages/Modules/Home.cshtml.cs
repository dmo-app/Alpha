using DMO.Alpha.Core.Runtime;
using DMO.Alpha.Web.Presentation;
using DMO.Alpha.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMO.Alpha.Web.Pages.Modules;

[Authorize]
public class HomeModel : PageModel
{
    private readonly ICurrentAccountContext _accountContext;
    private readonly ShellStateFactory _shellState;

    public HomeModel(ICurrentAccountContext accountContext, ShellStateFactory shellState)
    {
        _accountContext = accountContext;
        _shellState = shellState;
    }

    public ActorContext Actor => _accountContext.Current;

    public void OnGet()
    {
        // The Home module surface renders inside the shared shell. Its module
        // functions and side panel arrive with the module implementation.
        ViewData["Shell"] = _shellState.ForDestination("Home", new ModulePresentation("home", "Início"));
    }
}
