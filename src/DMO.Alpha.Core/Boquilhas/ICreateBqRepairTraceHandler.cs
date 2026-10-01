namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Handler do comando de criação de um bq_repair_trace_id ancorado a um
/// tool_id canónico existente. Nada é escrito no Job On nem no BqContext.
/// </summary>
public interface ICreateBqRepairTraceHandler
{
    Task<CreateBqRepairTraceOutcome> HandleAsync(
        CreateBqRepairTrace command,
        CancellationToken cancellationToken = default);
}
