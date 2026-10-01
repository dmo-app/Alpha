namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Resultado tipado do registo de um movimento de Boquilhas.
/// </summary>
public enum RegisterBqMovementResult
{
    Success,

    /// <summary>O bq_repair_trace_id não existe.</summary>
    TraceNotFound,

    /// <summary>O tool_id do trace não resolve a Tool canónica no registo Ferramentas.</summary>
    TraceToolNotRegistered,

    /// <summary>O tipo de movimento não é um dos três autorizados (saida, entrada, entrada_sem_reparacao).</summary>
    UnknownMovementType
}
