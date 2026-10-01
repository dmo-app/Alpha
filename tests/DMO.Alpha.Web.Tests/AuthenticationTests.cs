using System.Net;
using DMO.Alpha.Core.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DMO.Alpha.Web.Tests;

public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetState();
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private async Task<string> GetVerificationTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return HtmlFormHelpers.ExtractRequestVerificationToken(html)
            ?? throw new InvalidOperationException("No antiforgery token found on page.");
    }

    private async Task<HttpResponseMessage> LoginAsync(HttpClient client, string identifier, string password)
    {
        var token = await GetVerificationTokenAsync(client, "/login");
        var content = HtmlFormHelpers.BuildFormPost(
            ("identifier", identifier),
            ("password", password),
            ("returnUrl", "/"),
            ("__RequestVerificationToken", token));
        return await client.PostAsync("/login", content);
    }

    private void SeedAdmin(string email, string providerUserId, string password)
    {
        _factory.AuthProvider.AddAccount(email, providerUserId, password, email);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        dbContext.AdminAssociations.Add(new AdminAssociation
        {
            Id = Guid.NewGuid(),
            ProviderUserId = providerUserId,
            Email = email
        });
        dbContext.SaveChanges();
    }

    private void SeedUser(string operatorId, string name, string providerUserId, string password, bool requiresPasswordChange = false, string? profile = null)
    {
        var generatedEmail = $"{operatorId}@dmo.local";
        _factory.AuthProvider.AddAccount(generatedEmail, providerUserId, password, generatedEmail, requiresPasswordChange);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            OperatorId = operatorId,
            Name = name,
            ProviderUserId = providerUserId,
            RequiresPasswordChange = requiresPasswordChange,
            TemplateName = profile
        });
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task AdminLogin_Success_RedirectsToHome()
    {
        const string email = "admin@example.com";
        const string password = "AdminPass123!";
        SeedAdmin(email, "provider-admin-1", password);

        using var client = CreateClient();
        var response = await LoginAsync(client, email, password);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        var homeHtml = await home.Content.ReadAsStringAsync();
        Assert.Contains(email, homeHtml);
        Assert.Contains("ADMIN", homeHtml);
    }

    [Fact]
    public async Task AdminLogin_Failure_WrongPassword()
    {
        const string email = "admin@example.com";
        SeedAdmin(email, "provider-admin-2", "correct");

        using var client = CreateClient();
        var response = await LoginAsync(client, email, "wrong");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", html);
    }

    [Fact]
    public async Task AdminLogin_Failure_NotAssociated()
    {
        const string email = "orphan@example.com";
        _factory.AuthProvider.AddAccount(email, "provider-orphan", "password", email);

        using var client = CreateClient();
        var response = await LoginAsync(client, email, "password");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("not associated with an ADMIN account", html);
    }

    [Fact]
    public async Task UserLogin_Success_RedirectsToHome()
    {
        SeedUser("1234", "Joao Silva", "provider-user-1234", "UserPass1!");

        using var client = CreateClient();
        var response = await LoginAsync(client, "1234", "UserPass1!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        var html = await home.Content.ReadAsStringAsync();
        Assert.Contains("Joao Silva", html);
    }

    [Fact]
    public async Task UserLogin_PreservesLeadingZeros()
    {
        const string operatorId = "0042";
        SeedUser(operatorId, "Maria Souza", "provider-user-0042", "UserPass2!");

        using var client = CreateClient();
        var response = await LoginAsync(client, operatorId, "UserPass2!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        var html = await home.Content.ReadAsStringAsync();
        Assert.Contains("Maria Souza", html);
    }

    [Fact]
    public async Task UserLogin_Failure_WrongPassword()
    {
        SeedUser("9999", "Carlos", "provider-user-9999", "right");

        using var client = CreateClient();
        var response = await LoginAsync(client, "9999", "wrong");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", html);
    }

    [Fact]
    public async Task UserLogin_Failure_StandBy()
    {
        var generatedEmail = "0001@dmo.local";
        _factory.AuthProvider.AddAccount(generatedEmail, "provider-user-0001", "pass", generatedEmail);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            OperatorId = "0001",
            Name = "StandBy User",
            ProviderUserId = "provider-user-0001",
            IsStandBy = true
        });
        dbContext.SaveChanges();

        using var client = CreateClient();
        var response = await LoginAsync(client, "0001", "pass");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("stand-by", html);
    }

    [Fact]
    public async Task UserLogin_Failure_UnknownOperator()
    {
        // The provider knows the identity, but DMO has no User record for it.
        _factory.AuthProvider.AddAccount("5555@dmo.local", "provider-user-5555", "pass", "5555@dmo.local");

        using var client = CreateClient();
        var response = await LoginAsync(client, "5555", "pass");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unknown operator", html);
    }

    [Fact]
    public async Task ForcedPasswordChange_UserRequiresChange_BlocksOperationalPages()
    {
        SeedUser("1000", "Temp User", "provider-user-1000", "OldPass1!", requiresPasswordChange: true);

        using var client = CreateClient();
        var response = await LoginAsync(client, "1000", "OldPass1!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/change-password", response.Headers.Location?.OriginalString);

        var blocked = await client.GetAsync("/Modules/Home");
        Assert.Equal(HttpStatusCode.Redirect, blocked.StatusCode);
        Assert.Equal("/change-password", blocked.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task ForcedPasswordChange_UserSetsNewPassword_CanAccessApp()
    {
        SeedUser("2000", "Temp User", "provider-user-2000", "OldPass2!", requiresPasswordChange: true);

        using var client = CreateClient();
        var loginResponse = await LoginAsync(client, "2000", "OldPass2!");
        Assert.Equal("/change-password", loginResponse.Headers.Location?.OriginalString);

        var token = await GetVerificationTokenAsync(client, "/change-password");
        var content = HtmlFormHelpers.BuildFormPost(
            ("newPassword", "NewSecurePass1!"),
            ("confirmPassword", "NewSecurePass1!"),
            ("__RequestVerificationToken", token));
        var changeResponse = await client.PostAsync("/change-password", content);

        Assert.Equal(HttpStatusCode.Redirect, changeResponse.StatusCode);
        Assert.Equal("/", changeResponse.Headers.Location?.OriginalString);

        var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        var html = await home.Content.ReadAsStringAsync();
        Assert.Contains("Temp User", html);

        var module = await client.GetAsync("/Modules/Home");
        module.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ForcedPasswordChange_ProviderTemporaryPassword_CanBeCleared()
    {
        SeedUser("3000", "New Hire", "provider-user-3000", "TempPass1!", requiresPasswordChange: false);
        // Simulate the provider flagging the password as temporary by seeding the account with the flag.
        _factory.AuthProvider.AddAccount("3000@dmo.local", "provider-user-3000", "TempPass1!", isTemporaryPassword: true);

        using var client = CreateClient();
        var loginResponse = await LoginAsync(client, "3000", "TempPass1!");
        Assert.Equal("/change-password", loginResponse.Headers.Location?.OriginalString);

        var token = await GetVerificationTokenAsync(client, "/change-password");
        var content = HtmlFormHelpers.BuildFormPost(
            ("newPassword", "RealPass1!"),
            ("confirmPassword", "RealPass1!"),
            ("__RequestVerificationToken", token));
        var changeResponse = await client.PostAsync("/change-password", content);

        Assert.Equal(HttpStatusCode.Redirect, changeResponse.StatusCode);
        Assert.Equal("/", changeResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Unauthenticated_DeepLink_RedirectsToLoginWithReturnUrl()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/Modules/Home");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/login", location);
        Assert.Contains("ReturnUrl=%2FModules%2FHome", location);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        SeedUser("7777", "Logout User", "provider-user-7777", "Pass1!");

        using var client = CreateClient();
        var login = await LoginAsync(client, "7777", "Pass1!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var homeBefore = await client.GetAsync("/");
        homeBefore.EnsureSuccessStatusCode();

        var token = await GetVerificationTokenAsync(client, "/logout");
        var logout = await client.PostAsync("/logout", HtmlFormHelpers.BuildFormPost(("__RequestVerificationToken", token)));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/login", logout.Headers.Location?.OriginalString);

        var homeAfter = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, homeAfter.StatusCode);
        var loginRedirect = homeAfter.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/login", loginRedirect);
    }

    [Fact]
    public async Task AuthenticatedActorContext_ExposesProfileAndAdminFlag()
    {
        SeedUser("6000", "Profile User", "provider-user-6000", "Pass2!", profile: "Operador");

        using var client = CreateClient();
        var login = await LoginAsync(client, "6000", "Pass2!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var module = await client.GetAsync("/Modules/Home");
        module.EnsureSuccessStatusCode();
        var html = await module.Content.ReadAsStringAsync();
        Assert.Contains("Profile User", html);
        Assert.Contains("Profile: Operador", html);
        Assert.DoesNotContain("ADMIN", html);
    }

    [Fact]
    public async Task ModuleRoute_Home_IsReachableWhenAuthenticated()
    {
        SeedUser("8000", "Module User", "provider-user-8000", "Pass3!");

        using var client = CreateClient();
        var login = await LoginAsync(client, "8000", "Pass3!");
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var module = await client.GetAsync("/Modules/Home");
        Assert.Equal(HttpStatusCode.OK, module.StatusCode);
        var html = await module.Content.ReadAsStringAsync();
        Assert.Contains("Home Module", html);
    }
}
