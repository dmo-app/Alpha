using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tools;

/// <summary>
/// Implementação só de leitura da descoberta de candidatos de Tool para
/// seleção humana explícita. Aplica os filtros explícitos no servidor
/// (regra de queries cirúrgicas: nunca carrega o registo completo para o
/// cliente filtrar) com projeção mínima dos factos persistidos, AsNoTracking
/// e ordenação determinística por tool_id. Não cria Tools, não muta estado,
/// não deriva identidade de referência/lote/tipo e nunca auto-seleciona:
/// os filtros apenas reduzem os candidatos devolvidos.
/// </summary>
public sealed class ToolCandidateQuery : IToolCandidateQuery
{
    private readonly DmoDbContext _db;

    public ToolCandidateQuery(DmoDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ToolCandidate>> FindCandidatesAsync(
        ToolCandidateFilter? filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tool> query = _db.Tools.AsNoTracking();

        // Dimensões fechadas e explícitas, composta por E: cada dimensão
        // apenas reduz o conjunto. null/vazio = dimensão não aplicada, sem
        // exceção de argumento (estilo do IToolLookupQuery com valores
        // desconhecidos).
        if (filter is not null)
        {
            if (filter.Type is { } type)
            {
                query = query.Where(tool => tool.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(filter.Reference))
            {
                // Igualdade exata ao valor persistido: sem trimming, sem
                // case-insensitivity, sem contains — nenhuma semântica
                // difusa inventada.
                query = query.Where(tool => tool.Reference == filter.Reference);
            }

            if (!string.IsNullOrWhiteSpace(filter.Lot))
            {
                query = query.Where(tool => tool.Lot == filter.Lot);
            }
        }

        return await query
            .OrderBy(tool => tool.ToolId)
            .Select(tool => new ToolCandidate(tool.ToolId, tool.Type, tool.Reference, tool.Lot))
            .ToListAsync(cancellationToken);
    }
}
