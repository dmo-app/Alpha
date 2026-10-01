using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Core.Runtime;
using Microsoft.AspNetCore.Authentication;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Web.Pages;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly IAuthenticationProvider _provider;
    private readonly ICurrentAccountContext _accountContext;
    private readonly DmoDbContext _dbContext;
    private readonly DmoSignInService _signInService;

    public ChangePasswordModel(
        IAuthenticationProvider provider,
        ICurrentAccountContext accountContext,
        DmoDbContext dbContext,
        DmoSignInService signInService)
    {
        _provider = provider;
        _accountContext = accountContext;
        _dbContext = dbContext;
        _signInService = signInService;
    }

    public bool RequiresPasswordChange { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        RequiresPasswordChange = User.HasClaim(DmoClaimTypes.RequiresPasswordChange, bool.TrueString);
    }

    public async Task<IActionResult> OnPostAsync(string newPassword, string confirmPassword)
    {
        RequiresPasswordChange = User.HasClaim(DmoClaimTypes.RequiresPasswordChange, bool.TrueString);

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            ErrorMessage = "Informe e confirme a nova senha.";
            return Page();
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "As senhas não conferem.";
            return Page();
        }

        var actor = _accountContext.Current;
        if (!actor.IsAuthenticated)
        {
            return Challenge();
        }

        var providerUserId = User.FindFirst(DmoClaimTypes.ProviderUserId)?.Value;
        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            ErrorMessage = "Não foi possível identificar o usuário.";
            return Page();
        }

        var changeResult = await _provider.ChangePasswordAsync(providerUserId, newPassword);
        if (changeResult != ChangePasswordResult.Success)
        {
            ErrorMessage = changeResult == ChangePasswordResult.IdentityNotFound
                ? "Usuário não encontrado no provedor."
                : "O provedor de autenticação não pôde alterar a senha.";
            return Page();
        }

        if (!actor.IsAdmin)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.OperatorId == actor.OperatorId);

            if (user is not null)
            {
                user.RequiresPasswordChange = false;
                await _dbContext.SaveChangesAsync();
            }
        }

        // Re-issue the cookie with the new credential so the forced-change claim is gone.
        var identifier = actor.IsAdmin
            ? User.FindFirst(DmoClaimTypes.Email)?.Value
            : actor.OperatorId;

        if (string.IsNullOrWhiteSpace(identifier))
        {
            ErrorMessage = "Não foi possível refazer a autenticação.";
            return Page();
        }

        var signInResult = await _signInService.RefreshSignInAsync(identifier, newPassword, "/");
        if (!signInResult.Succeeded)
        {
            await HttpContext.SignOutAsync();
            ErrorMessage = "Senha alterada, mas a sessão não pôde ser renovada. Entre novamente.";
            return Page();
        }

        return Redirect(signInResult.RedirectUrl!);
    }
}
