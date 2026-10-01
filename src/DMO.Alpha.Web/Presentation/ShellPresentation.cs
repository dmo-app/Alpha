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
    IReadOnlyDictionary<string, ActionAvailability> ActionAvailability,
    SidepanelPresentation? Sidepanel = null,
    PlaneamentoSidepanelPresentation? PlaneamentoSidepanel = null);

/// <summary>
/// Shell-owned side panel state for the active destination. Presentation only:
/// the side panel is a shell component; which variant is open (or closed) is a
/// shared shell state, not a module page of its own.
/// </summary>
public enum ShellSidepanelKind
{
    /// <summary>No side panel is open for the active destination.</summary>
    None = 0,

    /// <summary>Planning side panel: current day first, then the following productions.</summary>
    Planeamento = 1,

    /// <summary>Boquilhas machine side panel: production machines with the current BQ context.</summary>
    Boquilhas = 2
}

public sealed record SidepanelPresentation(ShellSidepanelKind Kind, bool Visible);

/// <summary>
/// Data for the shell-owned Planeamento side panel. Presentation only: items are
/// discovery projections of planned productions (day label + human-identifying
/// detail), never a second source of production truth; selection leads to the
/// Job On destination by explicit user choice.
/// </summary>
public sealed record PlaneamentoSidepanelItem(string DayLabel, string Detail, string? Href);

public sealed record PlaneamentoSidepanelPresentation(
    string Title,
    string Subtitle,
    IReadOnlyList<PlaneamentoSidepanelItem> Productions);

public sealed record ModulePresentation(string Key, string Title);
public sealed record DestinationPresentation(string Label, string Href, bool Selected, bool Visible, bool Enabled);
public sealed record FunctionPresentation(
    string Key, string Label, string? Href, string? PanelTarget,
    bool Selected, bool Visible, bool Enabled, string CssClass = "");
public sealed record SessionPresentation(string Name, string Title, string LogoutHref);
public sealed record ActionAvailability(bool Visible, bool Enabled);
