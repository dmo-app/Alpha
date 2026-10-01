using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tools;

/// <summary>
/// Implementação só de leitura do surface nativo de resolução de uma Tool
/// pelo seu tool_id canónico exato, para o fluxo do Registo. A correspondência
/// é feita exclusivamente pelo ToolId (nunca por referência, lote ou tipo); a
/// projeção é mínima e usa AsNoTracking, sem criar Tools, sem mutar estado e
/// sem carregar dados não relacionados. Devolve null quando o tool_id é
/// desconhecido. O Tool.Type nunca é um gate.
/// </summary>
public sealed class ToolLookupQuery : IToolLookupQuery
{
    private readonly DmoDbContext _db;

    public ToolLookupQuery(DmoDbContext db)
    {
        _db = db;
    }

    public Task<ToolLookupResult?> FindByToolIdAsync(
        string toolId,
        CancellationToken cancellationToken = default)
    {
        return _db.Tools
            .AsNoTracking()
            .Where(t => t.ToolId == toolId)
            .Select(t => new ToolLookupResult(t.ToolId, t.Type, t.Reference, t.Lot))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
