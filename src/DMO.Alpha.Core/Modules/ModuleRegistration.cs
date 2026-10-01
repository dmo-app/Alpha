namespace DMO.Alpha.Core.Modules;

public sealed record ModuleRegistration(
    string Key,
    string Name,
    string Route,
    string? RequiredPermission = null);
