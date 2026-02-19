using Microsoft.EntityFrameworkCore;
using Pours.Infrastructure.Persistence;
using Xunit;

namespace Pours.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URL")
            ?? "Host=localhost;Port=5432;Database=pours_test;Username=postgres;Password=postgres";
    }

    public PourDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PourDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new PourDbContext(options);
    }

    public string ConnectionString => _connectionString;

    public async ValueTask InitializeAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
        await CleanTableAsync(context);
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = CreateDbContext();
        await CleanTableAsync(context);
    }

    private static async Task CleanTableAsync(PourDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "Pours" RESTART IDENTITY""");
    }
}
