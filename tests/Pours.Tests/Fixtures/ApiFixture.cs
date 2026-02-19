using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pours.Infrastructure.Persistence;
using Xunit;

namespace Pours.Tests.Fixtures;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestApiKey = "test-api-key-for-integration";

    private readonly string _connectionString;

    public ApiFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URL")
            ?? "Host=localhost;Port=5432;Database=pours_test;Username=postgres;Password=postgres";
    }

    public string ApiKey => TestApiKey;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Authentication:ApiKey", TestApiKey);
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext and Factory registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<PourDbContext>) ||
                            d.ServiceType == typeof(IDbContextFactory<PourDbContext>) ||
                            d.ServiceType.IsGenericType && 
                            d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextFactory<>))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContextFactory<PourDbContext>(options =>
                options.UseNpgsql(_connectionString));

            services.AddDbContext<PourDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PourDbContext>();
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "Pours" RESTART IDENTITY""");
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", TestApiKey);
        return client;
    }
}
