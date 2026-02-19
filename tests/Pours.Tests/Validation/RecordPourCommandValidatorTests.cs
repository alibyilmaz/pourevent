using FluentAssertions;
using Pours.Application.Commands;
using Pours.Application.Validators;
using Pours.Tests.Helpers;
using Xunit;

namespace Pours.Tests.Validation;

public sealed class RecordPourCommandValidatorTests
{
    private readonly RecordPourCommandValidator _validator = new();

    [Fact]
    public async Task ValidCommand_ShouldPassValidation()
    {
        var command = TestData.ValidCommand();

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyEventId_ShouldFailValidation()
    {
        var command = TestData.ValidCommand() with { EventId = Guid.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventId");
    }

    [Fact]
    public async Task EmptyDeviceId_ShouldFailValidation()
    {
        var command = TestData.ValidCommand() with { DeviceId = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeviceId");
    }

    [Fact]
    public async Task InvalidLocationId_ShouldFailValidation()
    {
        var command = TestData.ValidCommand() with { LocationId = "invalid-location" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LocationId");
    }

    [Fact]
    public async Task InvalidProductId_ShouldFailValidation()
    {
        var command = TestData.ValidCommand() with { ProductId = "invalid-beer" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public async Task InvalidVolumeMl_ShouldFailValidation()
    {
        var command = TestData.ValidCommand() with { VolumeMl = 999 };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VolumeMl");
    }

    [Fact]
    public async Task EndedAtBeforeStartedAt_ShouldFailValidation()
    {
        var now = DateTimeOffset.UtcNow;
        var command = TestData.ValidCommand() with
        {
            StartedAt = now,
            EndedAt = now.AddMinutes(-5)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndedAt");
    }

    [Theory]
    [InlineData("guinness")]
    [InlineData("ipa")]
    [InlineData("lager")]
    [InlineData("pilsner")]
    [InlineData("stout")]
    [InlineData("efes-pilsen")]
    [InlineData("efes-malt")]
    [InlineData("bomonti-filtresiz")]
    [InlineData("tuborg-gold")]
    [InlineData("tuborg-amber")]
    public async Task AllAllowedProductIds_ShouldPassValidation(string productId)
    {
        var command = TestData.ValidCommand() with { ProductId = productId };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("istanbul-kadikoy-01")]
    [InlineData("istanbul-besiktas-01")]
    [InlineData("izmir-alsancak-01")]
    [InlineData("ankara-cankaya-01")]
    [InlineData("london-soho-01")]
    public async Task AllAllowedLocationIds_ShouldPassValidation(string locationId)
    {
        var command = TestData.ValidCommand() with { LocationId = locationId };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(200)]
    [InlineData(250)]
    [InlineData(284)]
    [InlineData(330)]
    [InlineData(355)]
    [InlineData(400)]
    [InlineData(473)]
    [InlineData(500)]
    [InlineData(568)]
    [InlineData(1000)]
    public async Task AllAllowedVolumes_ShouldPassValidation(int volumeMl)
    {
        var command = TestData.ValidCommand() with { VolumeMl = volumeMl };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleInvalidFields_ShouldReturnMultipleErrors()
    {
        var command = new RecordPourCommand
        {
            EventId = Guid.Empty,
            DeviceId = "",
            LocationId = "invalid",
            ProductId = "invalid",
            StartedAt = default,
            EndedAt = default,
            VolumeMl = 0
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThan(3);
    }
}
