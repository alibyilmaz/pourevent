using System.Net;
using System.Text.Json;
using FluentAssertions;
using Pours.Tests.Fixtures;
using Xunit;

namespace Pours.Tests.Endpoints;

public sealed class HealthEndpointTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOkWithDbStatus()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().Should().Be("ok");
        root.GetProperty("db").GetString().Should().Be("ok");
    }
}
