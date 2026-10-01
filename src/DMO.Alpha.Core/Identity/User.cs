namespace DMO.Alpha.Core.Identity;

public sealed class User
{
    public Guid Id { get; init; }

    public required string Name { get; set; }

    /// <summary>
    /// Operator / BA Glass ID. Up to 4 digits. Leading zeros are preserved.
    /// </summary>
    public required string OperatorId { get; set; }

    /// <summary>
    /// The provider-owned technical authentication identity for this User.
    /// </summary>
    public required string ProviderUserId { get; set; }

    public bool RequiresPasswordChange { get; set; }

    public bool IsStandBy { get; set; }

    /// <summary>
    /// Human-facing profile/access label from the associated Access Template.
    /// May be null when no Template is assigned or when Templates are not yet configured.
    /// </summary>
    public string? TemplateName { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
