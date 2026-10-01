using System.Security.Claims;
using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Core.Identity;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Web.Services;

public sealed class DmoSignInService
{
    private readonly IAuthenticationProvider _provider;
    private readonly DmoDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DmoSignInService(
        IAuthenticationProvider provider,
        DmoDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _provider = provider;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SignInOutcome> SignInAsync(string identifier, string password, string? returnUrl = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP context.");

        var isEmail = identifier.Contains('@');

        // Map normal-User operator ID to the provider-facing identifier used by the configured provider.
        // For Supabase this is a generated email; for the InMemory seam it is the operator ID itself.
        var providerIdentifier = isEmail
            ? identifier
            : MapOperatorIdToProviderIdentifier(identifier);

        var authResult = await _provider.AuthenticateAsync(providerIdentifier, password);

        if (!authResult.Success)
        {
            return SignInOutcome.Failed(authResult.ErrorMessage ?? "Invalid credentials.");
        }

        List<Claim> claims;
        bool requiresPasswordChange;
        string displayName;

        if (isEmail)
        {
            var admin = await _dbContext.AdminAssociations
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ProviderUserId == authResult.ProviderUserId || a.Email == identifier);

            if (admin is null)
            {
                return SignInOutcome.Failed("This email is not associated with an ADMIN account.");
            }

            displayName = admin.Email;
            requiresPasswordChange = false;

            claims =
            [
                new Claim(DmoClaimTypes.ActorId, admin.Id.ToString()),
                new Claim(DmoClaimTypes.DisplayName, admin.Email),
                new Claim(DmoClaimTypes.IsAdmin, bool.TrueString),
                new Claim(DmoClaimTypes.Email, admin.Email),
                new Claim(DmoClaimTypes.ProviderUserId, authResult.ProviderUserId!)
            ];
        }
        else
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.OperatorId == identifier);

            if (user is null)
            {
                return SignInOutcome.Failed("Unknown operator identification.");
            }

            if (user.IsStandBy)
            {
                return SignInOutcome.Failed("Account is on stand-by.");
            }

            if (!string.Equals(user.ProviderUserId, authResult.ProviderUserId, StringComparison.Ordinal))
            {
                return SignInOutcome.Failed("Provider identity does not match the registered user.");
            }

            displayName = user.Name;
            requiresPasswordChange = user.RequiresPasswordChange || authResult.IsTemporaryPassword;

            claims =
            [
                new Claim(DmoClaimTypes.ActorId, user.Id.ToString()),
                new Claim(DmoClaimTypes.DisplayName, user.Name),
                new Claim(DmoClaimTypes.IsAdmin, bool.FalseString),
                new Claim(DmoClaimTypes.OperatorId, user.OperatorId),
                new Claim(DmoClaimTypes.ProviderUserId, authResult.ProviderUserId!)
            ];

            if (!string.IsNullOrWhiteSpace(user.TemplateName))
            {
                claims.Add(new Claim(DmoClaimTypes.ProfileLabel, user.TemplateName));
            }
        }

        if (requiresPasswordChange)
        {
            claims.Add(new Claim(DmoClaimTypes.RequiresPasswordChange, bool.TrueString));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

        return requiresPasswordChange
            ? SignInOutcome.RedirectTo("/change-password")
            : SignInOutcome.RedirectTo(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    public async Task<SignInOutcome> RefreshSignInAsync(string identifier, string password, string? returnUrl = null)
    {
        // Re-authenticate after a password change so the session reflects the new credential
        // and the forced-change requirement is cleared.
        return await SignInAsync(identifier, password, returnUrl);
    }

    private static string MapOperatorIdToProviderIdentifier(string operatorId)
    {
        // OPEN_PRODUCT_DECISION: the exact provider-side identifier for a normal User is not defined by Recovery.
        // This convention produces a deterministic email from the operator ID for Supabase-backed deployments.
        return $"{operatorId}@dmo.local";
    }
}

public sealed class SignInOutcome
{
    public bool Succeeded { get; private init; }
    public string? ErrorMessage { get; private init; }
    public string? RedirectUrl { get; private init; }

    public static SignInOutcome Failed(string message) => new() { ErrorMessage = message };

    public static SignInOutcome RedirectTo(string url) => new() { Succeeded = true, RedirectUrl = url };
}
