using DMO.Alpha.Core.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DMO.Alpha.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICurrentAccountContext _accountContext;

    public IndexModel(ICurrentAccountContext accountContext)
    {
        _accountContext = accountContext;
    }

    public ActorContext Actor => _accountContext.Current;

    public void OnGet()
    {
    }
}
