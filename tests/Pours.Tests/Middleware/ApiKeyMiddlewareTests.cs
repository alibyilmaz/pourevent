using System.Net;
using FluentAssertions;
using Pours.Tests.Fixtures;
using Xunit;

namespace Pours.Tests.Middleware;

public sealed class ApiKeyMiddlewareTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public ApiKeyMiddlewareTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MissingApiKey_ShouldReturn401()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/v1/taps/any-device/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WrongApiKey_ShouldReturn401()
    {
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "wrong-key");

        var response = await client.GetAsync("/v1/taps/any-device/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CorrectApiKey_ShouldNotReturn401()
    {
        var client = _fixture.CreateAuthenticatedClient();

        var response = await client.GetAsync("/v1/taps/any-device/summary?from=2024-01-01T00:00:00Z&to=2025-01-01T00:00:00Z");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldNotRequireApiKey()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
