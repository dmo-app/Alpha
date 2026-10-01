namespace DMO.Alpha.Core.Tools;

/// <summary>
/// Ferramenta canónica registada no Ferramentas. A identidade é o tool_id
/// emitido pelo backend; nunca é derivado de referência, lote, máquina ou
/// qualquer outro facto de descoberta de candidatos.
/// Um lote diferente é uma Tool diferente e portanto um tool_id diferente.
/// Entidade mínima do registo canónico; factos ainda não fechados no
/// blueprint (estado, máquinas/linhas compatíveis, processo) não são
/// persistidos aqui.
/// </summary>
public sealed class Tool
{
    public string ToolId { get; set; } = null!;

    public ToolType Type { get; set; }

    public string Reference { get; set; } = null!;

    public string Lot { get; set; } = null!;
}
