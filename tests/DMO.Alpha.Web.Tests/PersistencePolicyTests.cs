using DMO.Alpha.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DMO.Alpha.Web.Tests;

/// <summary>
/// Prova a política de persistência do runtime: fora de Development, a
/// aplicação nunca arranca com persistência InMemory silenciosa — ou usa a
/// ligação PostgreSQL configurada (ConnectionStrings:DmoDatabase), ou falha
/// de imediato com instruções concretas. InMemory fica reservado ao seam
/// explícito de Development e aos testes que constroem os seus próprios
/// contextos.
/// </summary>
public sealed class PersistencePolicyTests
{
    [Fact]
    public void Production_WithoutConfiguredDatabase_FailsFastInsteadOfSilentInMemory()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
        });

        var failure = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(failure);
        Assert.Contains("ConnectionStrings__DmoDatabase", failure.ToString());
    }

    [Fact]
    public void Production_WithConfiguredConnectionString_UsesDurableNpgsqlPersistence()
    {
        // Ligação nunca contactada por arrancar: apenas opções construídas.
        const string testConnectionString =
            "Host=localhost;Database=dmo_alpha_test;Username=dmo;Password=dmo";

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DmoDatabase"] = testConnectionString
                }));
        });

        using var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DmoDbContext>();

        // Configurada para persistência durável PostgreSQL, não InMemory.
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", db.Database.ProviderName);
    }
}
