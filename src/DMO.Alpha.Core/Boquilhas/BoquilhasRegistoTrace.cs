namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Read model purpose-specific do Registo de Boquilhas, resolvido por
/// bq_repair_trace_id canónico: a âncora tool_id permanente, os factos
/// referência/lote da Tool canónica quando resolvem no registo Ferramentas,
/// o bq_id opcional e o contexto de produção a ele ligado, os movimentos
/// persistidos do trace e apenas as quantidades físicas derivadas cujas
/// regras de movimento já fecham o cálculo: a quantidade legitimamente
/// fora do lote neste trace (regra fechada de matched/unmatched) e a
/// discrepância acumulada do trace como soma das discrepâncias
/// persistidas (null quando nenhum movimento tem discrepância). A
/// quantidade em casa não é derivada aqui: o total do lote não tem origem
/// canónica definida no blueprint atual.
/// </summary>
public sealed record BoquilhasRegistoTrace(
    int BqRepairTraceId,
    string ToolId,
    string? ToolReference,
    string? ToolLot,
    int? BqContextId,
    BoquilhasRegistoProductionContext? Production,
    IReadOnlyList<BoquilhasRegistoMovement> Movements,
    int QuantityLegitimatelyOut,
    int? AccumulatedDiscrepancy);
