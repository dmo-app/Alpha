namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Comando: associar um bq_repair_trace_id existente (ainda não resolvido)
/// ao seu bq_id (contexto BQ de produção existente). A associação dá apenas
/// contexto de produção: preserva o bq_repair_trace_id, o tool_id e os
/// movimentos existentes; não recria nem copia o trace; não altera JobOn
/// nem BqContext; não arranca, para, fecha ou reabre o trace. A escolha de
/// QUAL trace não resolvido associar pertence ao caller — este handler não
/// escolhe automaticamente entre candidatos.
/// </summary>
public sealed record AssociateBqRepairTraceToContext(
    int BqRepairTraceId,
    int BqContextId);
