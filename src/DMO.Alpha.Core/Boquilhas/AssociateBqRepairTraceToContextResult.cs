namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Resultado tipado da associação de um registo de reparação ao seu
/// contexto de produção BQ.
/// </summary>
public enum AssociateBqRepairTraceToContextResult
{
    Success,

    /// <summary>O bq_repair_trace_id não existe.</summary>
    TraceNotFound,

    /// <summary>O bq_id (BqContext) não existe.</summary>
    BqContextNotFound,

    /// <summary>
    /// O trace já está associado a um bq_id. Um trace associado nunca é
    /// reassociado: uma produção posterior nunca toma posse de um trace
    /// anterior (os retornos tardios ficam no trace de origem).
    /// </summary>
    TraceAlreadyAssociated,

    /// <summary>
    /// O bq_id já tem o seu único trace (um bq_id tem um
    /// bq_repair_trace_id). Recusa tipada em vez de violar o índice único.
    /// </summary>
    BqContextAlreadyTraced,

    /// <summary>
    /// O bq_id referencia um tool_id canónico diferente do tool_id
    /// permanente do trace. A correlação trace ↔ bq_id faz-se pelo mesmo
    /// tool_id canónico.
    /// </summary>
    TraceToolDoesNotMatchBqContextTool
}
