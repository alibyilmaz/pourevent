using FluentValidation;
using Pours.Application.Commands;
using Pours.Domain.Abstractions;
using Pours.Domain.Entities;

namespace Pours.Application.Services;

public sealed class PourService : IPourService
{
    private readonly IValidator<RecordPourCommand> _validator;
    private readonly IPourEventRepository _repository;

    public PourService(IValidator<RecordPourCommand> validator, IPourEventRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<RecordPourResult> RecordPourAsync(RecordPourCommand command, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return RecordPourResult.Invalid(validation.Errors
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                .ToList());
        }

        var entity = new PourEvent
        {
            EventId = command.EventId,
            DeviceId = command.DeviceId,
            LocationId = command.LocationId,
            ProductId = command.ProductId,
            StartedAt = command.StartedAt,
            EndedAt = command.EndedAt,
            VolumeMl = command.VolumeMl
        };

        var inserted = await _repository.TryAddIdempotentAsync(entity, ct);

        return inserted ? RecordPourResult.Created() : RecordPourResult.Duplicate();
    }
}
