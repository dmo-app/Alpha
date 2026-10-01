namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Comando: criar um registo de reparação de Boquilhas (bq_repair_trace_id)
/// ancorado a um tool_id canónico existente. Input: apenas o tool_id. O
/// trace nasce sem contexto de produção (bq_id = null); nenhum Job On é
/// inferido ou exigido. A cardinalidade de traces não resolvidos por
/// tool_id permanece em aberto no blueprint: vários podem coexistir.
/// </summary>
public sealed record CreateBqRepairTrace(string ToolId);
