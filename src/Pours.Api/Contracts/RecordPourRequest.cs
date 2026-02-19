namespace Pours.Api.Contracts;

public sealed class RecordPourRequest
{
    public Guid EventId { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string LocationId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset EndedAt { get; init; }
    public int VolumeMl { get; init; }
}
