namespace Pours.Api.Contracts;

public sealed class DeviceSummaryResponse
{
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
    public IReadOnlyList<ProductVolumeDto> ByProduct { get; init; } = [];
    public IReadOnlyList<LocationVolumeDto> ByLocation { get; init; } = [];
    public string? TopProduct { get; init; }
    public string? TopLocation { get; init; }
}

public sealed class ProductVolumeDto
{
    public required string ProductId { get; init; }
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
}

public sealed class LocationVolumeDto
{
    public required string LocationId { get; init; }
    public long TotalVolumeMl { get; init; }
    public int TotalPours { get; init; }
}
