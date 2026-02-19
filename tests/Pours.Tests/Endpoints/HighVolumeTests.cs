using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pours.Domain.Entities;
using Pours.Infrastructure.Persistence;
using Pours.Tests.Fixtures;
using System.Text.Json;
using Xunit;

namespace Pours.Tests.Endpoints;

public sealed class HighVolumeTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private const int RecordCount = 10_000;
    private const string DeviceId = "tap-highvol";
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;

    private static readonly string[] Products =
        ["guinness", "ipa", "lager", "pilsner", "stout"];

    private static readonly string[] Locations =
        ["istanbul-kadikoy-01", "istanbul-besiktas-01", "izmir-alsancak-01", "ankara-cankaya-01", "london-soho-01"];

    private static readonly int[] Volumes = [200, 250, 330, 500, 1000];

    public HighVolumeTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateAuthenticatedClient();
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PourDbContext>();
        await db.Database.EnsureCreatedAsync();
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions
            .ExecuteSqlRawAsync(db.Database, """TRUNCATE TABLE "Pours" RESTART IDENTITY""");

        await SeedHighVolumeDataAsync(db);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HighVolume_TotalsShouldBeCorrect()
    {
        var response = await _client.GetAsync(
            $"/v1/taps/{DeviceId}/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = await ParseAsync(response);

        var expectedTotalVolume = CalculateExpectedTotalVolume();
        root.GetProperty("totalPours").GetInt32().Should().Be(RecordCount);
        root.GetProperty("totalVolumeMl").GetInt64().Should().Be(expectedTotalVolume);
    }

    [Fact]
    public async Task HighVolume_TopProductShouldBeCorrect()
    {
        var response = await _client.GetAsync(
            $"/v1/taps/{DeviceId}/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        var root = await ParseAsync(response);

        var expectedTopProduct = CalculateExpectedTopProduct();
        root.GetProperty("topProduct").GetString().Should().Be(expectedTopProduct);
    }

    [Fact]
    public async Task HighVolume_TopLocationShouldBeCorrect()
    {
        var response = await _client.GetAsync(
            $"/v1/taps/{DeviceId}/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        var root = await ParseAsync(response);

        var expectedTopLocation = CalculateExpectedTopLocation();
        root.GetProperty("topLocation").GetString().Should().Be(expectedTopLocation);
    }

    [Fact]
    public async Task HighVolume_ByProductShouldBeSortedByVolumeDesc()
    {
        var response = await _client.GetAsync(
            $"/v1/taps/{DeviceId}/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        var root = await ParseAsync(response);
        var byProduct = root.GetProperty("byProduct");

        var volumes = new List<long>();
        foreach (var item in byProduct.EnumerateArray())
        {
            volumes.Add(item.GetProperty("totalVolumeMl").GetInt64());
        }

        volumes.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task HighVolume_ByLocationShouldBeSortedByVolumeDesc()
    {
        var response = await _client.GetAsync(
            $"/v1/taps/{DeviceId}/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        var root = await ParseAsync(response);
        var byLocation = root.GetProperty("byLocation");

        var volumes = new List<long>();
        foreach (var item in byLocation.EnumerateArray())
        {
            volumes.Add(item.GetProperty("totalVolumeMl").GetInt64());
        }

        volumes.Should().BeInDescendingOrder();
    }

    private static async Task SeedHighVolumeDataAsync(PourDbContext db)
    {
        var baseTime = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var entities = new List<PourEvent>(RecordCount);

        for (var i = 0; i < RecordCount; i++)
        {
            var productIndex = i % Products.Length;
            var locationIndex = i % Locations.Length;
            var volumeIndex = i % Volumes.Length;

            entities.Add(new PourEvent
            {
                EventId = Guid.NewGuid(),
                DeviceId = DeviceId,
                LocationId = Locations[locationIndex],
                ProductId = Products[productIndex],
                StartedAt = baseTime.AddSeconds(i),
                EndedAt = baseTime.AddSeconds(i + 5),
                VolumeMl = Volumes[volumeIndex]
            });
        }

        db.Pours.AddRange(entities);
        await db.SaveChangesAsync();
    }

    private static long CalculateExpectedTotalVolume()
    {
        long total = 0;
        for (var i = 0; i < RecordCount; i++)
        {
            total += Volumes[i % Volumes.Length];
        }
        return total;
    }

    private static string CalculateExpectedTopProduct()
    {
        var volumeByProduct = new Dictionary<string, long>();
        for (var i = 0; i < RecordCount; i++)
        {
            var product = Products[i % Products.Length];
            var volume = Volumes[i % Volumes.Length];
            volumeByProduct.TryGetValue(product, out var current);
            volumeByProduct[product] = current + volume;
        }
        return volumeByProduct.OrderByDescending(x => x.Value).First().Key;
    }

    private static string CalculateExpectedTopLocation()
    {
        var volumeByLocation = new Dictionary<string, long>();
        for (var i = 0; i < RecordCount; i++)
        {
            var location = Locations[i % Locations.Length];
            var volume = Volumes[i % Volumes.Length];
            volumeByLocation.TryGetValue(location, out var current);
            volumeByLocation[location] = current + volume;
        }
        return volumeByLocation.OrderByDescending(x => x.Value).First().Key;
    }

    private static async Task<JsonElement> ParseAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
