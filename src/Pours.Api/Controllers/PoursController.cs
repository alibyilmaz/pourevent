using Microsoft.AspNetCore.Mvc;
using Pours.Api.Contracts;
using Pours.Application.Commands;
using Pours.Application.Services;

namespace Pours.Api.Controllers;

[ApiController]
[Route("v1/pours")]
public sealed class PoursController : ControllerBase
{
    private readonly IPourService _pourService;

    public PoursController(IPourService pourService)
    {
        _pourService = pourService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordPour([FromBody] RecordPourRequest request, CancellationToken ct)
    {
        var command = new RecordPourCommand
        {
            EventId = request.EventId,
            DeviceId = request.DeviceId,
            LocationId = request.LocationId,
            ProductId = request.ProductId,
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            VolumeMl = request.VolumeMl
        };

        var result = await _pourService.RecordPourAsync(command, ct);

        if (result.IsInvalid)
            return BadRequest(new { errors = result.Errors.Select(e => new { field = e.Field, error = e.Message }) });

        return result.IsNew ? StatusCode(StatusCodes.Status201Created) : Ok();
    }
}
