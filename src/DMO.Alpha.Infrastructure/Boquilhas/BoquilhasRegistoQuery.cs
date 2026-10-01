using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Boquilhas;

/// <summary>
/// Serviço de leitura do Registo de Boquilhas: resolução cirúrgica por
/// bq_repair_trace_id (sem scans globais, projeções mínimas). Resolve os
/// factos referência/lote da Tool canónica quando existem no registo, o
/// contexto de produção Job On alcançável através do bq_id opcional, os
/// movimentos persistidos do trace e apenas as quantidades físicas
/// derivadas cujas regras de movimento já estão fechadas: a quantidade
/// legitimamente fora (cada saida acrescenta; cada retorno — entrada ou
/// entrada_sem_reparacao — devolve apenas a porção explicável) e a
/// discrepância acumulada (soma das discrepâncias persistidas, null
/// quando nenhuma). O Tool.Type nunca é um gate: o trace já transporta o
/// tool_id relevante.
/// </summary>
public sealed class BoquilhasRegistoQuery : IBoquilhasRegistoQuery
{
    private readonly DmoDbContext _db;

    public BoquilhasRegistoQuery(DmoDbContext db)
    {
        _db = db;
    }

    public async Task<BoquilhasRegistoTrace?> FindTraceAsync(
        int bqRepairTraceId,
        CancellationToken cancellationToken = default)
    {
        var trace = await _db.BqRepairTraces
            .AsNoTracking()
            .Where(t => t.Id == bqRepairTraceId)
            .Select(t => new { t.Id, t.ToolId, t.BqContextId })
            .FirstOrDefaultAsync(cancellationToken);
        if (trace is null)
        {
            return null;
        }

        // Factos da Tool canónica (referência/lote) quando resolvem; a
        // ausência não invalida o trace para leitura.
        var tool = await _db.Tools
            .AsNoTracking()
            .Where(tool => tool.ToolId == trace.ToolId)
            .Select(tool => new { tool.Reference, tool.Lot })
            .FirstOrDefaultAsync(cancellationToken);

        // Contexto de produção alcançável através do bq_id, apenas leitura.
        BoquilhasRegistoProductionContext? production = null;
        if (trace.BqContextId is int bqContextId)
        {
            var context = await _db.BqContexts
                .AsNoTracking()
                .Where(context => context.Id == bqContextId)
                .Select(context => new
                {
                    context.JobOn.Reference,
                    context.JobOn.ProductionNumber,
                    context.JobOn.Machine.Code
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (context is not null)
            {
                production = new BoquilhasRegistoProductionContext(
                    context.Reference,
                    context.ProductionNumber,
                    context.Code);
            }
        }

        var movements = await _db.BqMovements
            .AsNoTracking()
            .Where(movement => movement.BqRepairTraceId == bqRepairTraceId)
            .OrderBy(movement => movement.Id)
            .Select(movement => new
            {
                movement.Id,
                movement.Type,
                movement.Quantity,
                movement.Discrepancy
            })
            .ToListAsync(cancellationToken);

        // Quantidades físicas derivadas, apenas onde as regras já fecham o
        // cálculo: quantidade legitimamente fora (matched/unmatched) e
        // discrepância acumulada do trace.
        var quantityLegitimatelyOut = 0;
        var accumulatedDiscrepancy = 0;
        var hasDiscrepancy = false;
        foreach (var movement in movements)
        {
            if (movement.Type == BqMovementType.Saida)
            {
                quantityLegitimatelyOut += movement.Quantity;
            }
            else
            {
                var matched = Math.Min(movement.Quantity, quantityLegitimatelyOut);
                quantityLegitimatelyOut -= matched;
            }

            if (movement.Discrepancy is int discrepancy)
            {
                accumulatedDiscrepancy += discrepancy;
                hasDiscrepancy = true;
            }
        }

        return new BoquilhasRegistoTrace(
            trace.Id,
            trace.ToolId,
            tool?.Reference,
            tool?.Lot,
            trace.BqContextId,
            production,
            movements.Select(movement => new BoquilhasRegistoMovement(
                movement.Id,
                BqMovementTypeTokens.ToStorage(movement.Type),
                movement.Quantity,
                movement.Discrepancy)).ToList(),
            quantityLegitimatelyOut,
            hasDiscrepancy ? accumulatedDiscrepancy : null);
    }
}
