using Pours.Application.Commands;

namespace Pours.Tests.Helpers;

public static class TestData
{
    public static RecordPourCommand ValidCommand(Guid? eventId = null) => new()
    {
        EventId = eventId ?? Guid.NewGuid(),
        DeviceId = "tap-001",
        LocationId = "istanbul-kadikoy-01",
        ProductId = "guinness",
        StartedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
        EndedAt = DateTimeOffset.UtcNow,
        VolumeMl = 500
    };

    public static object ValidRequestBody(Guid? eventId = null) => new
    {
        eventId = eventId ?? Guid.NewGuid(),
        deviceId = "tap-001",
        locationId = "istanbul-kadikoy-01",
        productId = "guinness",
        startedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
        endedAt = DateTimeOffset.UtcNow,
        volumeMl = 500
    };
}
