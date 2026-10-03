namespace DMO.Alpha.Core.Tools;

/// <summary>
/// Filtros explícitos e fechados da descoberta de candidatos de Tool:
/// exatamente as três dimensões suportadas pela persistência canónica
/// corrente do registo Alpha (type, reference, lot) — factos que a fonte
/// funcional corrente do Ferramentas apresenta como visíveis para
/// distinguir candidatos e nunca como chave de identidade derivada. Cada
/// dimensão é opcional: null/vazio significa que a dimensão não é
/// aplicada (sem lançar exceção de argumento, ao estilo do
/// <see cref="IToolLookupQuery"/>).
/// A correspondência de reference/lot é a igualdade exata ao valor
/// persistido — sem trimming, sem case-insensitivity, sem
/// contains/semântica difusa. É o comportamento da implementação corrente
/// da query (determinístico, sem semântica difusa inventada); a fonte
/// funcional corrente não define regra canónica de correspondência
/// textual para esses factos.
/// Máquina/linha, quantidade (o lote contabilístico BQ), processo, estado
/// e associações explícitas de referência são factos de Tool correntes
/// na fonte funcional do Ferramentas (Core Tool data); o registo canónico
/// Alpha ainda não os persiste — lacuna de implementação do registo de
/// Tools, não decisão de domínio. Enquanto essa lacuna não for fechada,
/// esta seam não expõe dimensões de filtro para esses factos: os filtros
/// operam apenas sobre verdade persistida.
/// Os filtros reduzem candidatos — nunca determinam nem inferem
/// identidade: a identidade final é sempre o tool_id persistido da Tool
/// explicitamente selecionada pelo humano.
/// </summary>
public sealed record ToolCandidateFilter(
    ToolType? Type = null,
    string? Reference = null,
    string? Lot = null);
