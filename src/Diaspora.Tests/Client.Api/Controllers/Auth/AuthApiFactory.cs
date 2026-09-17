using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Diaspora.Tests.Client.Api.Controllers.Auth;

public class AuthApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DiasporaDb"] = _container.GetConnectionString(),
                ["Jwt:Key"] = "test-only-signing-key-not-used-anywhere-else-1234567890",
                ["Jwt:Issuer"] = "Diaspora.Tests",
                ["Jwt:Audience"] = "Diaspora.Tests.Audience",
                ["Jwt:AccessTokenMinutes"] = "60",
            });
        });
    }
}
