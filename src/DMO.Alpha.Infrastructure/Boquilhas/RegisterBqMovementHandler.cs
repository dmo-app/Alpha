using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Boquilhas;

/// <summary>
/// Regista um movimento real de Boquilhas num bq_repair_trace_id existente.
/// O movimento é acrescentado ao trace: não altera o Job On, não altera o
/// BqContext, não cria nem altera estados do trace e as datas de produção
/// não controlam o registo. A quantidade observada é registada na
/// totalidade — nunca truncada ou reescrita; um retorno que exceda a
/// quantidade explicável é aceite na totalidade (proibido rejeitar).
/// entrada_sem_reparacao é um token histórico distinto que, para efeitos de
/// contabilidade de quantidade, se comporta exatamente como entrada: reduz
/// a quantidade legitimamente fora e produz a mesma discrepância negativa
/// quando o retorno observado excede o explicável. Nenhum outro significado
/// (estado de reparação, qualidade, próxima ação, lifecycle) é inferido do
/// token. O trace já transporta o tool_id relevante; o registo não volta a
/// decidir se essa Tool é "realmente BQ" — o Tool.Type não é um gate
/// comportamental.
/// </summary>
public sealed class RegisterBqMovementHandler : IRegisterBqMovementHandler
{
    public const string Saida = "saida";
    public const string Entrada = "entrada";
    public const string EntradaSemReparacao = "entrada_sem_reparacao";

    private readonly DmoDbContext _db;

    public RegisterBqMovementHandler(DmoDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterBqMovementOutcome> HandleAsync(
        RegisterBqMovement command,
        CancellationToken cancellationToken = default)
    {
        // 1. O trace tem de existir.
        var trace = await _db.BqRepairTraces
            .FirstOrDefaultAsync(t => t.Id == command.BqRepairTraceId, cancellationToken);
        if (trace is null)
        {
            return Refused(RegisterBqMovementResult.TraceNotFound);
        }

        // 2. O tool_id do trace tem de resolver a Tool canónica no registo
        //    Ferramentas. Apenas existência no registo: o Tool.Type não é
        //    um gate comportamental — o trace já transporta o tool_id
        //    relevante para este registo de Boquilhas.
        var tool = await _db.Tools
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ToolId == trace.ToolId, cancellationToken);
        if (tool is null)
        {
            return Refused(RegisterBqMovementResult.TraceToolNotRegistered);
        }

        // 3. O tipo tem de ser um dos três tokens canónicos autorizados.
        var typeToken = command.MovementType;
        if (typeToken is not (Saida or Entrada or EntradaSemReparacao))
        {
            return Refused(RegisterBqMovementResult.UnknownMovementType);
        }

        var type = BqMovementTypeTokens.FromStorage(typeToken);

        // Discrepância: a regra fechada de retorno (matched/unmatched)
        // aplica-se de forma idêntica a entrada e entrada_sem_reparacao.
        int? discrepancy = null;
        if (type is BqMovementType.Entrada or BqMovementType.EntradaSemReparacao)
        {
            var history = await _db.BqMovements
                .AsNoTracking()
                .Where(m => m.BqRepairTraceId == trace.Id)
                .OrderBy(m => m.Id)
                .ToListAsync(cancellationToken);

            // Quantidade legitimamente fora do lote neste trace: as saídas
            // acrescentam; cada retorno (entrada ou entrada_sem_reparacao)
            // devolve apenas a porção explicável (matched).
            var outstanding = 0;
            foreach (var movement in history)
            {
                if (movement.Type == BqMovementType.Saida)
                {
                    outstanding += movement.Quantity;
                }
                else
                {
                    var matched = Math.Min(movement.Quantity, outstanding);
                    outstanding -= matched;
                }
            }

            var matchedQuantity = Math.Min(command.Quantity, outstanding);
            var unmatchedQuantity = command.Quantity - matchedQuantity;
            // Sem quantidade não explicada não há discrepância (em branco,
            // nunca zero como facto).
            discrepancy = unmatchedQuantity > 0 ? -unmatchedQuantity : null;
        }

        // O movimento é acrescentado ao trace com a quantidade observada na
        // totalidade e o token de movimento distinto persistido.
        var persisted = new BqMovement
        {
            BqRepairTraceId = trace.Id,
            Type = type,
            Quantity = command.Quantity,
            Discrepancy = discrepancy
        };
        _db.BqMovements.Add(persisted);
        await _db.SaveChangesAsync(cancellationToken);

        return new RegisterBqMovementOutcome(
            RegisterBqMovementResult.Success,
            new RegisteredBqMovement(
                persisted.Id,
                trace.Id,
                typeToken,
                command.Quantity,
                discrepancy));
    }

    private static RegisterBqMovementOutcome Refused(RegisterBqMovementResult result) =>
        new(result);
}
