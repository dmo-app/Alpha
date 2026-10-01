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
            // Default to EF Core InMemory only when no connection string is supplied.
            // This supports local development and integration tests without PostgreSQL.
            services.AddDbContext<DmoDbContext>(options =>
                options.UseInMemoryDatabase("DmoAlpha"));
        }
        else
        {
            services.AddDbContext<DmoDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(DmoDbContext).Assembly.FullName)));
        }

        return services;
    }
}
