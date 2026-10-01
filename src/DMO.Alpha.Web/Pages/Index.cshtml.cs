using DMO.Alpha.Core.Runtime;
using DMO.Alpha.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMO.Alpha.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICurrentAccountContext _accountContext;
    private readonly ShellStateFactory _shellState;

    public IndexModel(ICurrentAccountContext accountContext, ShellStateFactory shellState)
    {
        _accountContext = accountContext;
        _shellState = shellState;
    }

    public ActorContext Actor => _accountContext.Current;

    public void OnGet()
    {
        // The post-login landing renders inside the shared shell. No side panel
        // and no module functions exist for the landing destination yet.
        ViewData["Shell"] = _shellState.ForDestination("Home", module: null);
    }
}
