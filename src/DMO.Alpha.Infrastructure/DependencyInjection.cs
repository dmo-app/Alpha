using DMO.Alpha.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMO.Alpha.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDmoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DmoDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // No silent InMemory fallback: infrastructure wiring without a
            // configured PostgreSQL connection string is a configuration
            // error, not a durable persistence setup. Tests that need
            // InMemory construct DmoDbContext with UseInMemoryDatabase
            // explicitly.
            throw new InvalidOperationException(
                "ConnectionStrings:DmoDatabase is not configured. AddDmoInfrastructure " +
                "requires a PostgreSQL connection string (environment variable " +
                "ConnectionStrings__DmoDatabase).");
        }

        services.AddDbContext<DmoDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(DmoDbContext).Assembly.FullName)));

        return services;
    }
}
