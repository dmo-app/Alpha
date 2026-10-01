using DMO.Alpha.Core.Production;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Production;

/// <summary>
/// Implementação purpose-specific de <see cref="IProductionContextQuery"/>.
/// Usa projecção directa sobre as tabelas reais; não carrega entidades completas
/// nem introduz um aggregate/entidade genérica Context.
/// </summary>
public sealed class ProductionContextQuery : IProductionContextQuery
{
    private readonly DmoDbContext _db;

    public ProductionContextQuery(DmoDbContext db)
    {
        _db = db;
    }

    public async Task<ProductionContextReadModel?> GetAsync(int jobOnId, CancellationToken cancellationToken = default)
    {
        var query = from j in _db.JobOns
                    where j.Id == jobOnId
                    join m in _db.Machines on j.MachineId equals m.Id
                    join cm in _db.CmContexts on j.Id equals cm.JobOnId into cmGroup
                    from cm in cmGroup.DefaultIfEmpty()
                    join mf in _db.MfContexts on j.Id equals mf.JobOnId into mfGroup
                    from mf in mfGroup.DefaultIfEmpty()
                    join bq in _db.BqContexts on j.Id equals bq.JobOnId into bqGroup
                    from bq in bqGroup.DefaultIfEmpty()
                    select new ProductionContextReadModel(
                        j.Id,
                        j.Reference,
                        j.ProductionNumber,
                        m.Code,
                        cm != null ? cm.Id : (int?)null,
                        cm != null ? cm.ToolId : null,
                        mf != null ? mf.Id : (int?)null,
                        mf != null ? mf.ToolId : null,
                        bq != null ? bq.Id : (int?)null,
                        bq != null ? bq.ToolId : null);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
