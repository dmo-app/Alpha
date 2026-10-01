namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Handler do comando de associação de um bq_repair_trace_id existente ao
/// seu bq_id. A associação não altera JobOn, BqContext, tool_id, nem os
/// movimentos existentes, e não controla lifecycle do trace.
/// </summary>
public interface IAssociateBqRepairTraceToContextHandler
{
    Task<AssociateBqRepairTraceToContextOutcome> HandleAsync(
        AssociateBqRepairTraceToContext command,
        CancellationToken cancellationToken = default);
}
