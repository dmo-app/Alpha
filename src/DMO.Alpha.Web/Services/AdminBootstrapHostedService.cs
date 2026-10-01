using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Core.Identity;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Web.Services;

/// <summary>
/// Creates the ADMIN association in DMO when a matching Supabase Auth identity already exists.
/// DMO never creates the provider identity itself.
/// This is a Module 1 bootstrap seam; the Admin module will replace it with a Setup UI.
/// </summary>
public sealed class AdminBootstrapHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminBootstrapHostedService> _logger;

    public AdminBootstrapHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<AdminBootstrapHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var adminEmail = _configuration["Dmo:AdminEmail"];
        var adminProviderUserId = _configuration["Dmo:AdminProviderUserId"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminProviderUserId))
        {
            _logger.LogInformation("No ADMIN bootstrap configuration provided. Skipping association seed.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IAuthenticationProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<DmoDbContext>();

        var identityExists = await provider.IdentityExistsAsync(adminProviderUserId, cancellationToken);
        if (!identityExists)
        {
            _logger.LogWarning("ADMIN email {AdminEmail} is configured but no matching provider identity exists. No association created.", adminEmail);
            return;
        }

        var existing = await dbContext.AdminAssociations
            .AsNoTracking()
            .AnyAsync(a => a.ProviderUserId == adminProviderUserId, cancellationToken);

        if (existing)
        {
            _logger.LogInformation("ADMIN association already exists.");
            return;
        }

        dbContext.AdminAssociations.Add(new AdminAssociation
        {
            Id = Guid.NewGuid(),
            ProviderUserId = adminProviderUserId,
            Email = adminEmail
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("ADMIN association created for {AdminEmail}.", adminEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
