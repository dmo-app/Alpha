namespace DMO.Alpha.Core.Production;

/// <summary>
/// Read model purpose-specific para o contexto mínimo de uma produção.
/// Não é uma entidade nem um aggregate genérico; é uma projecção plana
/// devolvida pela query <see cref="IProductionContextQuery"/>.
/// </summary>
public sealed record ProductionContextReadModel(
    int JobOnId,
    string Reference,
    string ProductionNumber,
    string Machine,
    int? CmId,
    string? CmToolId,
    int? MfId,
    string? MfToolId,
    int? BqId,
    string? BqToolId);
