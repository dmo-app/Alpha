namespace DMO.Alpha.Core.Production;

/// <summary>
/// Job On (registo de produção). Entidade mínima para a proof de contexto;
/// não pretende ser um modelo de domínio completo.
/// </summary>
public sealed class JobOn
{
    public int Id { get; set; }

    public string Reference { get; set; } = null!;

    public string ProductionNumber { get; set; } = null!;

    public int MachineId { get; set; }

    public Machine Machine { get; set; } = null!;

    public CmContext? CmContext { get; set; }

    public MfContext? MfContext { get; set; }

    public BqContext? BqContext { get; set; }
}
