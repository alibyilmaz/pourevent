using Microsoft.AspNetCore.Mvc;
using Pours.Api.Contracts;
using Pours.Application.Services;

namespace Pours.Api.Controllers;

[ApiController]
[Route("v1/taps")]
public sealed class TapsController : ControllerBase
{
    private readonly ISummaryService _summaryService;

    public TapsController(ISummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    [HttpGet("{deviceId}/summary")]
    public async Task<IActionResult> GetSummary(
        [FromRoute] string deviceId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        if (from > to)
        {
            return BadRequest(new
            {
                errors = new[]
                {
                    new { field = "to", error = "to must be greater than or equal to from." }
                }
            });
        }

        var result = await _summaryService.GetDeviceSummaryAsync(deviceId, from, to, ct);

        return Ok(new DeviceSummaryResponse
        {
            TotalVolumeMl = result.TotalVolumeMl,
            TotalPours = result.TotalPours,
            ByProduct = result.ByProduct.Select(p => new ProductVolumeDto
            {
                ProductId = p.ProductId,
                TotalVolumeMl = p.TotalVolumeMl,
                TotalPours = p.TotalPours
            }).ToList(),
            ByLocation = result.ByLocation.Select(l => new LocationVolumeDto
            {
                LocationId = l.LocationId,
                TotalVolumeMl = l.TotalVolumeMl,
                TotalPours = l.TotalPours
            }).ToList(),
            TopProduct = result.TopProduct,
            TopLocation = result.TopLocation
        });
    }
}
