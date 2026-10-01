namespace DMO.Alpha.Web.Presentation;

// Presentation values only. This contract neither resolves authorization nor
// models production, movements, workflow or any other domain entity.
public sealed record ShellPresentation(
    ModulePresentation? CurrentModule,
    string ActiveShellDestination,
    string? ActiveFunctionKey,
    IReadOnlyList<FunctionPresentation> Functions,
    DestinationPresentation Planeamento,
    SessionPresentation Session,
    IReadOnlyDictionary<string, ActionAvailability> ActionAvailability);

public sealed record ModulePresentation(string Key, string Title);
public sealed record DestinationPresentation(string Label, string Href, bool Selected, bool Visible, bool Enabled);
public sealed record FunctionPresentation(
    string Key, string Label, string? Href, string? PanelTarget,
    bool Selected, bool Visible, bool Enabled, string CssClass = "");
public sealed record SessionPresentation(string Name, string Title, string LogoutHref);
public sealed record ActionAvailability(bool Visible, bool Enabled);
