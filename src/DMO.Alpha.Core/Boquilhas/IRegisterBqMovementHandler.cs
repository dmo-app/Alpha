namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Handler do comando de registo de um movimento de Boquilhas num
/// bq_repair_trace_id existente. O movimento é acrescentado ao trace;
/// nada é escrito no Job On, no BqContext ou em estados do trace.
/// </summary>
public interface IRegisterBqMovementHandler
{
    Task<RegisterBqMovementOutcome> HandleAsync(
        RegisterBqMovement command,
        CancellationToken cancellationToken = default);
}
