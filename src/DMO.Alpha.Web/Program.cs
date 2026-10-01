using Microsoft.AspNetCore.DataProtection;
using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Core.Modules;
using DMO.Alpha.Core.Runtime;
using DMO.Alpha.Infrastructure.Authentication;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// Authentication provider
// -------------------------------------------------------------------------
var authProviderName = builder.Configuration.GetValue<string>("Dmo:AuthenticationProvider");
var useSupabase = string.Equals(authProviderName, "Supabase", StringComparison.OrdinalIgnoreCase);

if (useSupabase)
{
    builder.Services.Configure<SupabaseAuthOptions>(builder.Configuration.GetSection("Dmo:Supabase"));
    builder.Services.AddSingleton<IValidateOptions<SupabaseAuthOptions>, SupabaseAuthOptionsValidator>();
    builder.Services.AddOptions<SupabaseAuthOptions>().ValidateOnStart();
    builder.Services.AddHttpClient<IAuthenticationProvider, SupabaseAuthenticationProvider>();
}
else
{
    // InMemory seam for local development and deterministic integration tests.
    builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
    {
        var provider = new InMemoryAuthenticationProvider();
        var section = builder.Configuration.GetSection("Dmo:InMemoryAccounts");

        foreach (var child in section.GetChildren())
        {
            var identifier = child["Identifier"];
            var password = child["Password"];

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                continue;
            }

            var providerUserId = child["ProviderUserId"] ?? identifier;
            var email = child["Email"];
            var isTemporary = bool.TryParse(child["IsTemporaryPassword"], out var t) && t;

            provider.AddAccount(identifier, providerUserId, password, email, isTemporary);
        }

        return provider;
    });
}

// -------------------------------------------------------------------------
// Data access
// -------------------------------------------------------------------------
builder.Services.AddDbContext<DmoDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DmoDatabase");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("DmoAlphaModule1");
    }
});

// -------------------------------------------------------------------------
// Runtime services
// -------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentAccountContext, HttpContextCurrentAccountContext>();
builder.Services.AddSingleton<IModuleRegistry, DefaultModuleRegistry>();
builder.Services.AddScoped<DmoSignInService>();
builder.Services.AddHostedService<AdminBootstrapHostedService>();

// -------------------------------------------------------------------------
// Authentication & authorization
// -------------------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "DmoAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery();

// -------------------------------------------------------------------------
// Razor Pages
// -------------------------------------------------------------------------
builder.Services.AddRazorPages();
// All local application runtime files stay in Alpha.
var dataProtection = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "temp", "data-protection")));
if (OperatingSystem.IsWindows()) dataProtection.ProtectKeysWithDpapi();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Force password change before any operational page when the claim is present.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true &&
        user.HasClaim(DmoClaimTypes.RequiresPasswordChange, bool.TrueString))
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (!path.StartsWith("/change-password", StringComparison.Ordinal) &&
            !path.StartsWith("/logout", StringComparison.Ordinal) &&
            !path.StartsWith("/login", StringComparison.Ordinal) &&
            !path.StartsWith("/css/", StringComparison.Ordinal) &&
            !path.StartsWith("/js/", StringComparison.Ordinal) &&
            !path.StartsWith("/_", StringComparison.Ordinal))
        {
            context.Response.Redirect("/change-password");
            return;
        }
    }

    await next();
});

app.MapRazorPages();

app.Run();
