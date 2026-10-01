using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Boquilhas;

/// <summary>
/// Cria um registo de reparação de Boquilhas (bq_repair_trace_id) ancorado a
/// um tool_id canónico existente no registo Ferramentas. O trace nasce com
/// bq_id não resolvido (null): nenhum Job On é inferido, exigido ou
/// criado. O Tool.Type não é um gate comportamental — apenas a existência
/// da Tool no registo canónico é verificada. A cardinalidade de traces não
/// resolvidos por tool_id está em aberto no blueprint: nenhum limite é
/// imposto e vários podem coexistir.
/// </summary>
public sealed class CreateBqRepairTraceHandler : ICreateBqRepairTraceHandler
{
    private readonly DmoDbContext _db;

    public CreateBqRepairTraceHandler(DmoDbContext db)
    {
        _db = db;
    }

    public async Task<CreateBqRepairTraceOutcome> HandleAsync(
        CreateBqRepairTrace command,
        CancellationToken cancellationToken = default)
    {
        // O tool_id tem de resolver a Tool canónica no registo Ferramentas.
        var tool = await _db.Tools
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ToolId == command.ToolId, cancellationToken);
        if (tool is null)
        {
            return Refused(CreateBqRepairTraceResult.ToolNotFound);
        }

        // Novo bq_repair_trace_id canónico, ancorado ao tool_id, sem
        // contexto de produção.
        var trace = new BqRepairTrace
        {
            ToolId = command.ToolId,
            BqContextId = null
        };
        _db.BqRepairTraces.Add(trace);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateBqRepairTraceOutcome(
            CreateBqRepairTraceResult.Success,
            new CreatedBqRepairTrace(trace.Id, trace.ToolId, trace.BqContextId));
    }

    private static CreateBqRepairTraceOutcome Refused(CreateBqRepairTraceResult result) =>
        new(result);
}
