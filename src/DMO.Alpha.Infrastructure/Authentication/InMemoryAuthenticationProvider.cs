using DMO.Alpha.Core.Authentication;

namespace DMO.Alpha.Infrastructure.Authentication;

/// <summary>
/// Deterministic authentication provider seam for local development and integration tests.
/// Credentials are held only by this test seam; DMO product code never persists passwords.
/// </summary>
public sealed class InMemoryAuthenticationProvider : IAuthenticationProvider
{
    private readonly Dictionary<string, InMemoryAccount> _accounts = new(StringComparer.OrdinalIgnoreCase);

    public void AddAccount(string identifier, string providerUserId, string password, string? email = null, bool isTemporaryPassword = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentException.ThrowIfNullOrEmpty(providerUserId);
        ArgumentException.ThrowIfNullOrEmpty(password);

        _accounts[identifier] = new InMemoryAccount
        {
            Identifier = identifier,
            ProviderUserId = providerUserId,
            Email = email ?? identifier,
            Password = password,
            IsTemporaryPassword = isTemporaryPassword
        };
    }

    public Task<AuthenticateResult> AuthenticateAsync(string identifier, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentException.ThrowIfNullOrEmpty(password);

        if (!_accounts.TryGetValue(identifier, out var account))
        {
            return Task.FromResult(AuthenticateResult.Failed("Invalid credentials."));
        }

        if (account.Password != password)
        {
            return Task.FromResult(AuthenticateResult.Failed("Invalid credentials."));
        }

        return Task.FromResult(AuthenticateResult.Succeeded(
            account.ProviderUserId,
            account.Email ?? identifier,
            account.IsTemporaryPassword));
    }

    public Task<ChangePasswordResult> ChangePasswordAsync(string providerUserId, string newPassword, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerUserId);
        ArgumentException.ThrowIfNullOrEmpty(newPassword);

        var account = _accounts.Values.FirstOrDefault(a =>
            string.Equals(a.ProviderUserId, providerUserId, StringComparison.Ordinal));

        if (account is null)
        {
            return Task.FromResult(ChangePasswordResult.IdentityNotFound);
        }

        account.Password = newPassword;
        account.IsTemporaryPassword = false;
        return Task.FromResult(ChangePasswordResult.Success);
    }

    public Task<bool> IdentityExistsAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (_accounts.ContainsKey(identifier))
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(_accounts.Values.Any(a =>
            string.Equals(a.ProviderUserId, identifier, StringComparison.OrdinalIgnoreCase)));
    }

    public void Clear()
    {
        _accounts.Clear();
    }
}
