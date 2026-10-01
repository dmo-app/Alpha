namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Serviço de leitura consumer-specific do surface Registo de Boquilhas,
/// resolvido por bq_repair_trace_id canónico. Não é um repositório
/// genérico de Boquilhas nem um rail de produção.
/// </summary>
public interface IBoquilhasRegistoQuery
{
    /// <summary>
    /// Devolve o read model do trace canónico pedido, ou null quando o
    /// bq_repair_trace_id não existe.
    /// </summary>
    Task<BoquilhasRegistoTrace?> FindTraceAsync(int bqRepairTraceId, CancellationToken cancellationToken = default);
}
