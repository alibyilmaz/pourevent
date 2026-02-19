using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pours.Infrastructure.Persistence;

namespace Pours.Api.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    private readonly PourDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(PourDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(ct);
            if (canConnect)
            {
                return Ok(new { status = "ok", db = "ok" });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "degraded", db = "error" });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "degraded", db = "error" });
        }
    }
}
