using Pours.Domain.Abstractions;
using Pours.Domain.Models;

namespace Pours.Application.Services;

public sealed class SummaryService : ISummaryService
{
    private readonly IPourSummaryRepository _repository;

    public SummaryService(IPourSummaryRepository repository)
    {
        _repository = repository;
    }

    public Task<DeviceSummaryResult> GetDeviceSummaryAsync(
        string deviceId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        return _repository.GetDeviceSummaryAsync(deviceId, from, to, ct);
    }
}
