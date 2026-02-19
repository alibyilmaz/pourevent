namespace Pours.Domain.Entities;

public class PourEvent
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
    public int VolumeMl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
