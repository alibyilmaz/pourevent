using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pours.Domain.Entities;
using Pours.Infrastructure.Persistence;
using Pours.Tests.Fixtures;
using Xunit;

namespace Pours.Tests.Endpoints;

public sealed class DeviceSummaryTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;

    public DeviceSummaryTests(ApiFixture fixture)
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
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task EmptyDevice_ShouldReturnZeros()
    {
        var response = await _client.GetAsync(
            "/v1/taps/nonexistent-device/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("totalVolumeMl").GetInt64().Should().Be(0);
        root.GetProperty("totalPours").GetInt32().Should().Be(0);
        root.GetProperty("byProduct").GetArrayLength().Should().Be(0);
        root.GetProperty("byLocation").GetArrayLength().Should().Be(0);
        root.GetProperty("topProduct").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("topLocation").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task SinglePour_ShouldReturnCorrectTotals()
    {
        await SeedPourAsync("tap-100", "guinness", "istanbul-kadikoy-01", 500,
            new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync(
            "/v1/taps/tap-100/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        root.GetProperty("totalVolumeMl").GetInt64().Should().Be(500);
        root.GetProperty("totalPours").GetInt32().Should().Be(1);
        root.GetProperty("topProduct").GetString().Should().Be("guinness");
        root.GetProperty("topLocation").GetString().Should().Be("istanbul-kadikoy-01");
    }

    [Fact]
    public async Task MultipleProducts_ShouldReturnByProductSortedByVolumeDesc()
    {
        var baseTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await SeedPourAsync("tap-200", "guinness", "istanbul-kadikoy-01", 500, baseTime);
        await SeedPourAsync("tap-200", "guinness", "istanbul-kadikoy-01", 500, baseTime.AddMinutes(1));
        await SeedPourAsync("tap-200", "ipa", "istanbul-kadikoy-01", 330, baseTime.AddMinutes(2));

        var response = await _client.GetAsync(
            "/v1/taps/tap-200/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        root.GetProperty("totalVolumeMl").GetInt64().Should().Be(1330);
        root.GetProperty("totalPours").GetInt32().Should().Be(3);

        var byProduct = root.GetProperty("byProduct");
        byProduct.GetArrayLength().Should().Be(2);
        byProduct[0].GetProperty("productId").GetString().Should().Be("guinness");
        byProduct[0].GetProperty("totalVolumeMl").GetInt64().Should().Be(1000);
        byProduct[1].GetProperty("productId").GetString().Should().Be("ipa");
        byProduct[1].GetProperty("totalVolumeMl").GetInt64().Should().Be(330);
    }

    [Fact]
    public async Task MultipleLocations_ShouldReturnByLocationSortedByVolumeDesc()
    {
        var baseTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await SeedPourAsync("tap-300", "guinness", "istanbul-kadikoy-01", 500, baseTime);
        await SeedPourAsync("tap-300", "guinness", "istanbul-besiktas-01", 1000, baseTime.AddMinutes(1));

        var response = await _client.GetAsync(
            "/v1/taps/tap-300/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        var byLocation = root.GetProperty("byLocation");
        byLocation.GetArrayLength().Should().Be(2);
        byLocation[0].GetProperty("locationId").GetString().Should().Be("istanbul-besiktas-01");
        byLocation[0].GetProperty("totalVolumeMl").GetInt64().Should().Be(1000);
        byLocation[1].GetProperty("locationId").GetString().Should().Be("istanbul-kadikoy-01");
        byLocation[1].GetProperty("totalVolumeMl").GetInt64().Should().Be(500);
    }

    [Fact]
    public async Task TopProduct_ShouldBeHighestVolume()
    {
        var baseTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await SeedPourAsync("tap-400", "lager", "istanbul-kadikoy-01", 200, baseTime);
        await SeedPourAsync("tap-400", "stout", "istanbul-kadikoy-01", 1000, baseTime.AddMinutes(1));
        await SeedPourAsync("tap-400", "ipa", "istanbul-kadikoy-01", 500, baseTime.AddMinutes(2));

        var response = await _client.GetAsync(
            "/v1/taps/tap-400/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        root.GetProperty("topProduct").GetString().Should().Be("stout");
    }

    [Fact]
    public async Task TopLocation_ShouldBeHighestVolume()
    {
        var baseTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);

        await SeedPourAsync("tap-500", "guinness", "istanbul-kadikoy-01", 200, baseTime);
        await SeedPourAsync("tap-500", "guinness", "london-soho-01", 1000, baseTime.AddMinutes(1));

        var response = await _client.GetAsync(
            "/v1/taps/tap-500/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        root.GetProperty("topLocation").GetString().Should().Be("london-soho-01");
    }

    [Fact]
    public async Task TimeRangeFiltering_ShouldOnlyIncludeMatchingRecords()
    {
        var insideRange = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var outsideRange = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);

        await SeedPourAsync("tap-600", "guinness", "istanbul-kadikoy-01", 500, insideRange);
        await SeedPourAsync("tap-600", "guinness", "istanbul-kadikoy-01", 500, outsideRange);

        var response = await _client.GetAsync(
            "/v1/taps/tap-600/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");
        var root = await ParseResponseAsync(response);

        root.GetProperty("totalVolumeMl").GetInt64().Should().Be(500);
        root.GetProperty("totalPours").GetInt32().Should().Be(1);
    }

    private async Task SeedPourAsync(string deviceId, string productId, string locationId, int volumeMl,
        DateTimeOffset startedAt)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PourDbContext>();
        db.Pours.Add(new PourEvent
        {
            EventId = Guid.NewGuid(),
            DeviceId = deviceId,
            LocationId = locationId,
            ProductId = productId,
            StartedAt = startedAt,
            EndedAt = startedAt.AddSeconds(10),
            VolumeMl = volumeMl
        });
        await db.SaveChangesAsync();
    }

    private static async Task<JsonElement> ParseResponseAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
