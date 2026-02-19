using Pours.Domain.Entities;

namespace Pours.Domain.Abstractions;

public interface IPourEventRepository
{
    Task<bool> TryAddIdempotentAsync(PourEvent entity, CancellationToken ct = default);
}
