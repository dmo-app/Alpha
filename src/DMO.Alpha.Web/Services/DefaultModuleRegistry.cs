using DMO.Alpha.Core.Modules;

namespace DMO.Alpha.Web.Services;

public sealed class DefaultModuleRegistry : IModuleRegistry
{
    public IReadOnlyList<ModuleRegistration> AvailableModules { get; } = new List<ModuleRegistration>
    {
        // Module 1 registers the minimal Home module to prove route reachability.
        // Real operational modules are added here when their implementation starts.
        new("home", "Início", "/Modules/Home")
    };
}
