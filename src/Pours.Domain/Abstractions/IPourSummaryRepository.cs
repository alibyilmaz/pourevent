using Pours.Domain.Models;

namespace Pours.Domain.Abstractions;

public interface IPourSummaryRepository
{
    Task<DeviceSummaryResult> GetDeviceSummaryAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default);
}
