namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Resultado tipado da criação de um registo de reparação de Boquilhas.
/// </summary>
public enum CreateBqRepairTraceResult
{
    Success,

    /// <summary>O tool_id não resolve a Tool canónica no registo Ferramentas.</summary>
    ToolNotFound
}
