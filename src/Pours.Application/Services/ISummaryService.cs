using Pours.Domain.Models;

namespace Pours.Application.Services;

public interface ISummaryService
{
    Task<DeviceSummaryResult> GetDeviceSummaryAsync(
        string deviceId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
