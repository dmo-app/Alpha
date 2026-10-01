using DMO.Alpha.Core.Production;
using DMO.Alpha.Core.Tools;

namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Registo de reparação de Boquilhas (repair trace) para um contexto BQ de
/// produção. A identidade canónica é o bq_repair_trace_id, emitido pelo
/// backend: um trace agrupa todos os ciclos de movimento de um bq_id e não
/// é criado por viagem de reparação.
/// O trace pertence à Tool física BQ através do tool_id canónico (âncora
/// permanente, usada para correlacionar um trace pré-produção com o bq_id
/// que mais tarde o referenciar). A associação a bq_id dá apenas contexto
/// de produção: pode ser null antes de o Job On existir; a associação não
/// recria, substitui nem reinicia o trace. Não existe estado open/closed
/// nem lifecycle — Job On não arranca nem para um trace.
/// Entidade mínima de identidade e âncoras; os movimentos pertencem a um
/// slice futuro.
/// </summary>
public sealed class BqRepairTrace
{
    public int Id { get; set; }

    public string ToolId { get; set; } = null!;

    public Tool Tool { get; set; } = null!;

    /// <summary>
    /// bq_id — o contexto BQ (BqContext) usado num jobon_id. Null enquanto
    /// o trace não tem contexto de produção (pré-produção, não resolvido).
    /// </summary>
    public int? BqContextId { get; set; }

    public BqContext? BqContext { get; set; }
}
