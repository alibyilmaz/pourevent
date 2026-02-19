using Microsoft.EntityFrameworkCore;
using Pours.Domain.Abstractions;
using Pours.Domain.Models;
using Pours.Infrastructure.Persistence;

namespace Pours.Infrastructure.Repositories;

public sealed class PourSummaryRepository : IPourSummaryRepository
{
    private readonly IDbContextFactory<PourDbContext> _dbContextFactory;

    public PourSummaryRepository(IDbContextFactory<PourDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DeviceSummaryResult> GetDeviceSummaryAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        // Run both queries in parallel using separate DbContext instances
        var byProductTask = GetByProductAsync(deviceId, from, to, ct);
        var byLocationTask = GetByLocationAsync(deviceId, from, to, ct);

        await Task.WhenAll(byProductTask, byLocationTask);

        var byProduct = await byProductTask;
        var byLocation = await byLocationTask;

        var totalVolumeMl = byProduct.Sum(p => p.TotalVolumeMl);
        var totalPours = byProduct.Sum(p => p.TotalPours);

        return new DeviceSummaryResult
        {
            TotalVolumeMl = totalVolumeMl,
            TotalPours = totalPours,
            ByProduct = byProduct,
            ByLocation = byLocation,
            TopProduct = byProduct.FirstOrDefault()?.ProductId,
            TopLocation = byLocation.FirstOrDefault()?.LocationId
        };
    }

    private async Task<List<ProductVolume>> GetByProductAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Pours
            .AsNoTracking()
            .Where(p => p.DeviceId == deviceId && p.StartedAt >= from && p.StartedAt <= to)
            .GroupBy(p => p.ProductId)
            .Select(g => new ProductVolume
            {
                ProductId = g.Key,
                TotalVolumeMl = g.Sum(x => (long)x.VolumeMl),
                TotalPours = g.Count()
            })
            .OrderByDescending(x => x.TotalVolumeMl)
            .ToListAsync(ct);
    }

    private async Task<List<LocationVolume>> GetByLocationAsync(
        string deviceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Pours
            .AsNoTracking()
            .Where(p => p.DeviceId == deviceId && p.StartedAt >= from && p.StartedAt <= to)
            .GroupBy(p => p.LocationId)
            .Select(g => new LocationVolume
            {
                LocationId = g.Key,
                TotalVolumeMl = g.Sum(x => (long)x.VolumeMl),
                TotalPours = g.Count()
            })
            .OrderByDescending(x => x.TotalVolumeMl)
            .ToListAsync(ct);
    }
}
