namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Movimento de Boquilhas: um evento real de quantidade que pertence a um
/// registo de reparação (bq_repair_trace_id). A identidade canónica é o
/// movement_id, emitido pelo backend, distinta de bq_repair_trace_id,
/// bq_id, tool_id e de qualquer identidade de registo/audit.
/// A quantidade observada é registada na totalidade — nunca truncada,
/// compensada ou reescrita. A discrepância produzida pelo movimento é um
/// facto histórico (por exemplo -2 para duas BQ não explicadas numa
/// Entrada); null significa movimento normal, sem discrepância (em
/// branco, nunca zero exibido como facto). Nenhum estado de trace é criado
/// ou alterado por movimentos, e nada é escrito de volta no Job On.
/// Entidade mínima de identidade e factos explicitamente definidos; datas,
/// reparador e razão/motivo não estão definidos no blueprint atual e não
/// são persistidos neste slice.
/// </summary>
public sealed class BqMovement
{
    public int Id { get; set; }

    public int BqRepairTraceId { get; set; }

    public BqRepairTrace Trace { get; set; } = null!;

    public BqMovementType Type { get; set; }

    /// <summary>Quantidade observada, registada na totalidade.</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Discrepância produzida por este movimento (negativa: quantidade
    /// observada não explicada). Null = movimento normal, sem discrepância.
    /// </summary>
    public int? Discrepancy { get; set; }
}
