using DMO.Alpha.Core.Authentication;
using DMO.Alpha.Infrastructure.Authentication;
using DMO.Alpha.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DMO.Alpha.Web.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryAuthenticationProvider AuthProvider { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace the authentication provider with the deterministic seam.
            services.AddSingleton<IAuthenticationProvider>(AuthProvider);

            // Replace EF configuration with a single in-memory database instance
            // that is reset between tests.
            const string databaseName = "DmoAlphaTestDatabase";
            services.AddScoped(_ =>
                new DbContextOptionsBuilder<DmoDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .Options);

            services.AddScoped(sp =>
                new DmoDbContext(sp.GetRequiredService<DbContextOptions<DmoDbContext>>()));
        });
    }

    public void ResetState()
    {
        AuthProvider.Clear();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DmoDbContext>();
        dbContext.AdminAssociations.RemoveRange(dbContext.AdminAssociations);
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.SaveChanges();
    }
}
