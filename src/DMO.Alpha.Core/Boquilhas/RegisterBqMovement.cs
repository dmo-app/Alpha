namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Comando: registar um movimento real de Boquilhas num bq_repair_trace_id
/// existente. Contém apenas os factos suportados pelo blueprint atual: o
/// trace de destino, o tipo de movimento (token canónico) e a quantidade
/// observada. A discrepância é produzida pelo backend conforme as regras de
/// matched/unmatched e nunca é fornecida pelo cliente. Sem campos de
/// data/ator: o blueprint atual não define o contrato persistido desses
/// factos para movimentos.
/// </summary>
public sealed record RegisterBqMovement(
    int BqRepairTraceId,
    string MovementType,
    int Quantity);
