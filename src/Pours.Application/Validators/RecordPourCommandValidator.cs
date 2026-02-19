using FluentValidation;
using Pours.Domain.Constants;

namespace Pours.Application.Validators;

public sealed class RecordPourCommandValidator : AbstractValidator<Commands.RecordPourCommand>
{
    public RecordPourCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("EventId is required and must be a valid UUID.");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("DeviceId is required.");

        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("LocationId is required.")
            .Must(id => AllowedValues.LocationIds.Contains(id))
            .WithMessage("LocationId is not in the allowed list.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.")
            .Must(id => AllowedValues.ProductIds.Contains(id))
            .WithMessage("ProductId is not in the allowed list.");

        RuleFor(x => x.VolumeMl)
            .Must(v => AllowedValues.Volumes.Contains(v))
            .WithMessage("VolumeMl is not in the allowed list.");

        RuleFor(x => x.StartedAt)
            .NotEmpty()
            .WithMessage("StartedAt is required.");

        RuleFor(x => x.EndedAt)
            .NotEmpty()
            .WithMessage("EndedAt is required.")
            .GreaterThanOrEqualTo(x => x.StartedAt)
            .WithMessage("EndedAt must be greater than or equal to StartedAt.");
    }
}
