using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pours.Infrastructure.Persistence;
using Pours.Tests.Fixtures;
using Xunit;

namespace Pours.Tests.Endpoints;

public sealed class PourIngestionTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;

    public PourIngestionTests(ApiFixture fixture)
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
    public async Task ValidPour_ShouldReturn201()
    {
        var body = new
        {
            eventId = Guid.NewGuid(),
            deviceId = "tap-001",
            locationId = "istanbul-kadikoy-01",
            productId = "guinness",
            startedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            endedAt = DateTimeOffset.UtcNow,
            volumeMl = 500
        };

        var response = await _client.PostAsJsonAsync("/v1/pours", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DuplicateEventId_ShouldReturn200()
    {
        var eventId = Guid.NewGuid();
        var body = new
        {
            eventId,
            deviceId = "tap-001",
            locationId = "istanbul-kadikoy-01",
            productId = "guinness",
            startedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            endedAt = DateTimeOffset.UtcNow,
            volumeMl = 500
        };

        var first = await _client.PostAsJsonAsync("/v1/pours", body);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/v1/pours", body);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DuplicateEventId_ShouldNotInsertSecondRecord()
    {
        var eventId = Guid.NewGuid();
        var body = new
        {
            eventId,
            deviceId = "tap-001",
            locationId = "istanbul-kadikoy-01",
            productId = "guinness",
            startedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            endedAt = DateTimeOffset.UtcNow,
            volumeMl = 500
        };

        await _client.PostAsJsonAsync("/v1/pours", body);
        await _client.PostAsJsonAsync("/v1/pours", body);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PourDbContext>();
        var count = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .CountAsync(db.Pours.Where(p => p.EventId == eventId));

        count.Should().Be(1);
    }

    [Fact]
    public async Task InvalidPayload_ShouldReturn400()
    {
        var body = new
        {
            eventId = Guid.Empty,
            deviceId = "",
            locationId = "invalid-location",
            productId = "invalid-beer",
            startedAt = DateTimeOffset.UtcNow,
            endedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            volumeMl = 999
        };

        var response = await _client.PostAsJsonAsync("/v1/pours", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MissingApiKey_ShouldReturn401()
    {
        var unauthClient = _fixture.CreateClient();
        var body = new
        {
            eventId = Guid.NewGuid(),
            deviceId = "tap-001",
            locationId = "istanbul-kadikoy-01",
            productId = "guinness",
            startedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            endedAt = DateTimeOffset.UtcNow,
            volumeMl = 500
        };

        var response = await unauthClient.PostAsJsonAsync("/v1/pours", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
