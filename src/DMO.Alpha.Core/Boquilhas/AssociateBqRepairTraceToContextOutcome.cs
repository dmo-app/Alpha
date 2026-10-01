namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Resultado da associação: o outcome tipado e, em caso de sucesso, o read
/// model purpose-specific do trace associado (mesma identidade, mesmo
/// tool_id, agora com bq_id resolvido).
/// </summary>
public sealed record AssociateBqRepairTraceToContextOutcome(
    AssociateBqRepairTraceToContextResult Result,
    AssociatedBqRepairTrace? Trace = null);

/// <summary>
/// Read model purpose-specific do trace associado ao contexto de produção.
/// </summary>
public sealed record AssociatedBqRepairTrace(
    int BqRepairTraceId,
    string ToolId,
    int BqContextId);
