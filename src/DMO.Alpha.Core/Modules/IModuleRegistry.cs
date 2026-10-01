namespace DMO.Alpha.Core.Modules;

public interface IModuleRegistry
{
    IReadOnlyList<ModuleRegistration> AvailableModules { get; }
}
