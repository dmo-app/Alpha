using Microsoft.Extensions.Options;

namespace DMO.Alpha.Infrastructure.Authentication;

public sealed class SupabaseAuthOptionsValidator : IValidateOptions<SupabaseAuthOptions>
{
    public ValidateOptionsResult Validate(string? name, SupabaseAuthOptions options)
    {
        var failures = new List<string>(3);

        if (string.IsNullOrWhiteSpace(options.Url))
        {
            failures.Add($"{nameof(SupabaseAuthOptions.Url)} is required.");
        }
        else if (!Uri.IsWellFormedUriString(options.Url, UriKind.Absolute))
        {
            failures.Add($"{nameof(SupabaseAuthOptions.Url)} must be an absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(options.AnonKey))
        {
            failures.Add($"{nameof(SupabaseAuthOptions.AnonKey)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ServiceKey))
        {
            failures.Add($"{nameof(SupabaseAuthOptions.ServiceKey)} is required.");
        }

        if (failures.Count == 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(failures);
    }
}
