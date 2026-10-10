using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Authorize]
[Route("api/locations")]
[EnableRateLimiting("external")]
public sealed class LocationController : ControllerBase
{
    private readonly IGeoService _geoService;
    public LocationController(IGeoService geoService) => _geoService = geoService;

    [HttpGet("search")]
    public async Task<IActionResult> Search(string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3) return BadRequest();
        return Ok(await _geoService.SearchAsync(q.Trim(), cancellationToken));
    }

    [HttpGet("reverse")]
    public async Task<IActionResult> Reverse(double lat, double lon, CancellationToken cancellationToken)
    {
        if (lat is < -90 or > 90 || lon is < -180 or > 180) return BadRequest();
        var result = await _geoService.ReverseAsync(lat, lon, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
