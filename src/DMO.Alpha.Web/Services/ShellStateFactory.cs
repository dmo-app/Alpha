using DMO.Alpha.Core.Runtime;
using DMO.Alpha.Web.Presentation;

namespace DMO.Alpha.Web.Services;

/// <summary>
/// Builds the shared shell state for native shell pages from the authenticated
/// actor context. Presentation only: it resolves no authorization, owns no
/// domain facts and exposes no capability beyond the current build registry.
/// Module pages with real read models replace the presentation values they own.
/// </summary>
public sealed class ShellStateFactory
{
    private readonly ICurrentAccountContext _accountContext;

    public ShellStateFactory(ICurrentAccountContext accountContext)
    {
        _accountContext = accountContext;
    }

    /// <summary>
    /// Shell state for a destination without side panel or module functions.
    /// Planeamento stays visible but unavailable until its destination surface
    /// is implemented and registered as available.
    /// </summary>
    public ShellPresentation ForDestination(string destinationKey, ModulePresentation? module)
    {
        var actor = _accountContext.Current;

        return new ShellPresentation(
            module,
            destinationKey,
            ActiveFunctionKey: null,
            Functions: Array.Empty<FunctionPresentation>(),
            Planeamento: new DestinationPresentation(
                Label: "Planeamento",
                Href: string.Empty,
                Selected: false,
                Visible: true,
                Enabled: false),
            Session: new SessionPresentation(
                Name: actor.IsAuthenticated ? actor.DisplayName : string.Empty,
                Title: actor.ProfileLabel ?? string.Empty,
                LogoutHref: "/logout"),
            ActionAvailability: new Dictionary<string, ActionAvailability>(),
            Sidepanel: new SidepanelPresentation(ShellSidepanelKind.None, Visible: false));
    }
}
