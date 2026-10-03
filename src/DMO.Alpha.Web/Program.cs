using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.DataProtection;
using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Core.Boquilhas;
using DMO.Alpha.Core.Modules;
using DMO.Alpha.Core.Runtime;
using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Authentication;
using DMO.Alpha.Infrastructure.Boquilhas;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Infrastructure.Tools;
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
// Durable persistence: PostgreSQL (incl. Supabase) configured through
// configuration only — ConnectionStrings:DmoDatabase, fed by the environment
// variable ConnectionStrings__DmoDatabase, user secrets or the gitignored
// appsettings.Development.json. No credential is ever committed.
//
// InMemory is an explicit Development-only seam: integration tests construct
// their own InMemory contexts, and a deployed environment never silently
// pretends in-memory persistence is durable.
builder.Services.AddDbContext<DmoDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DmoDatabase");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(DmoDbContext).Assembly.FullName));
    }
    else if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("DmoAlphaModule1");
    }
    else
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DmoDatabase is not configured. Durable PostgreSQL persistence " +
            "is required outside Development; set the environment variable ConnectionStrings__DmoDatabase.");
    }
});

// -------------------------------------------------------------------------
// Runtime services
// -------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentAccountContext, HttpContextCurrentAccountContext>();
builder.Services.AddSingleton<IModuleRegistry, DefaultModuleRegistry>();
builder.Services.AddSingleton<ShellStateFactory>();
builder.Services.AddScoped<DmoSignInService>();
// Boquilhas write path: registo de um movimento num bq_repair_trace_id existente.
builder.Services.AddScoped<IRegisterBqMovementHandler, RegisterBqMovementHandler>();
// Boquilhas read path: read model consumer-specific do surface Registo.
builder.Services.AddScoped<IBoquilhasRegistoQuery, BoquilhasRegistoQuery>();
// Boquilhas write path: criar um trace ancorado a um tool_id e associá-lo
// mais tarde ao seu bq_id de produção.
builder.Services.AddScoped<ICreateBqRepairTraceHandler, CreateBqRepairTraceHandler>();
builder.Services.AddScoped<IAssociateBqRepairTraceToContextHandler, AssociateBqRepairTraceToContextHandler>();
// Tools read path: resolução canónica e só de leitura de uma Tool pelo seu tool_id exato.
builder.Services.AddScoped<IToolLookupQuery, ToolLookupQuery>();
// Tools read path: descoberta de candidatos para seleção humana explícita
// (filtros explícitos aplicados no servidor; apenas reduzem candidatos).
builder.Services.AddScoped<IToolCandidateQuery, ToolCandidateQuery>();
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

// Razor writes pt-PT text (à, ç, ·, —) literally instead of numeric entities:
// the default HtmlEncoder entity-encodes every non-Basic-Latin character.
// HTML-sensitive characters (<, >, &, ", ') remain encoded regardless.
builder.Services.AddSingleton<HtmlEncoder>(
    HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.GeneralPunctuation));

// -------------------------------------------------------------------------
// Razor Pages
// -------------------------------------------------------------------------
builder.Services.AddRazorPages();
// All local application runtime files stay in Alpha.
var dataProtection = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "temp", "data-protection")));
if (OperatingSystem.IsWindows()) dataProtection.ProtectKeysWithDpapi();

var app = builder.Build();

// Fail fast at startup when a deployed environment is missing the durable
// database configuration: resolving the options executes the policy above,
// so a misconfigured environment cannot boot and then lose writes on first
// use. (In Development this resolves the explicit InMemory seam.)
using (var scope = app.Services.CreateScope())
{
    _ = scope.ServiceProvider.GetRequiredService<DbContextOptions<DmoDbContext>>();
}

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
