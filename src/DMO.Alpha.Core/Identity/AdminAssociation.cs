namespace DMO.Alpha.Core.Identity;

/// <summary>
/// DMO's association of an already-existing external authentication identity with the ADMIN function.
/// DMO never creates the external Auth identity itself.
/// </summary>
public sealed class AdminAssociation
{
    public Guid Id { get; init; }

    public required string ProviderUserId { get; init; }

    public required string Email { get; init; }

    public DateTimeOffset AssociatedAt { get; init; } = DateTimeOffset.UtcNow;
}
