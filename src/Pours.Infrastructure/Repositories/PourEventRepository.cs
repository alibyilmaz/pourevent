using Microsoft.EntityFrameworkCore;
using Pours.Domain.Abstractions;
using Pours.Domain.Entities;
using Pours.Infrastructure.Persistence;

namespace Pours.Infrastructure.Repositories;

public sealed class PourEventRepository : IPourEventRepository
{
    private readonly PourDbContext _dbContext;

    public PourEventRepository(PourDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TryAddIdempotentAsync(PourEvent entity, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO "Pours" ("EventId", "DeviceId", "LocationId", "ProductId", "StartedAt", "EndedAt", "VolumeMl", "CreatedAt")
            VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, now())
            ON CONFLICT ("EventId") DO NOTHING
            """;

        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
            sql,
            [entity.EventId, entity.DeviceId, entity.LocationId, entity.ProductId, entity.StartedAt.UtcDateTime, entity.EndedAt.UtcDateTime, entity.VolumeMl],
            ct);

        return rowsAffected > 0;
    }
}
