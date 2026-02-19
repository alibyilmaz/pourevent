namespace Pours.Domain.Models;

public sealed class DeviceSummaryResult
{
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
    public IReadOnlyList<ProductVolume> ByProduct { get; init; } = [];
    public IReadOnlyList<LocationVolume> ByLocation { get; init; } = [];
    public string? TopProduct { get; init; }
    public string? TopLocation { get; init; }
}

public sealed class ProductVolume
{
    public required string ProductId { get; init; }
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
}

public sealed class LocationVolume
{
    public required string LocationId { get; init; }
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
}
