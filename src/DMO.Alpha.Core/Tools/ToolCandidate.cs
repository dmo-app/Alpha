namespace DMO.Alpha.Core.Tools;

/// <summary>
/// Read model purpose-specific da descoberta de candidatos de Tool para
/// seleção humana explícita (picker Ferramentas/Boquilhas). Expõe apenas
/// os factos atualmente persistidos no registo canónico que uma pessoa
/// precisa para distinguir candidatos: o tool_id (ÚNICA identidade
/// canónica, emitida pelo backend) e os factos descritivos
/// type/reference/lot, projetados exatamente como estão persistidos. Um
/// lote diferente é uma Tool diferente e portanto um tool_id diferente:
/// candidatos com a mesma referência e lotes diferentes nunca colapsam.
/// O candidato nunca transporta qualquer noção de seleção, pré-seleção ou
/// preferência — a seleção é sempre um ato humano explícito sobre o
/// tool_id persistido, mesmo quando só resta um candidato.
/// </summary>
public sealed record ToolCandidate(
    string ToolId,
    ToolType Type,
    string Reference,
    string Lot);
