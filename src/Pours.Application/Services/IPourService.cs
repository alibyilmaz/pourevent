using Pours.Application.Commands;

namespace Pours.Application.Services;

public interface IPourService
{
    Task<RecordPourResult> RecordPourAsync(RecordPourCommand command, CancellationToken ct = default);
}
