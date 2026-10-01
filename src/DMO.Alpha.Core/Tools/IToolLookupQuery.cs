namespace DMO.Alpha.Core.Tools;

/// <summary>
/// Surface nativo, canónico e só de leitura para resolver uma Tool pelo seu
/// tool_id canónico exato no fluxo do Registo. Não é um repositório genérico:
/// nunca deriva identidade de referência/lote/tipo, nunca cria Tools nem muta
/// estado, e devolve null quando o tool_id é desconhecido.
/// </summary>
public interface IToolLookupQuery
{
    /// <summary>
    /// Devolve o read model da Tool cujo tool_id canónico coincide exatamente
    /// com o pedido, ou null quando esse tool_id não existe no registo. Um
    /// toolId nulo, vazio ou constituído apenas por espaços é tratado como um
    /// tool_id desconhecido e devolve null (sem lançar exceção de argumento).
    /// </summary>
    Task<ToolLookupResult?> FindByToolIdAsync(string toolId, CancellationToken cancellationToken = default);
}
