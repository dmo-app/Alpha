namespace DMO.Alpha.Infrastructure.Authentication;

public sealed class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;

    public string AnonKey { get; set; } = string.Empty;

    public string ServiceKey { get; set; } = string.Empty;
}
