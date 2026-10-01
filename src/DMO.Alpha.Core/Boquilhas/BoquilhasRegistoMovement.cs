namespace DMO.Alpha.Core.Boquilhas;

/// <summary>
/// Read model purpose-specific do Registo: um movimento persistido do
/// trace, tal como o Registo o apresenta — movement_id canónico, token
/// canónico persistido (saida / entrada / entrada_sem_reparacao),
/// quantidade observada registada na totalidade e a discrepância produzida
/// pelo backend (null quando o movimento não tem discrepância).
/// </summary>
public sealed record BoquilhasRegistoMovement(
    int MovementId,
    string MovementType,
    int Quantity,
    int? Discrepancy);
