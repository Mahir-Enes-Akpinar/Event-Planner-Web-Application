using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[AllowAnonymous]
[Route("map-tiles")]
public sealed class MapTilesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MapTilesController> _logger;

    public MapTilesController(IHttpClientFactory httpClientFactory, ILogger<MapTilesController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("{z:int}/{x:int}/{y:int}.png")]
    [ResponseCache(Duration = 60 * 60 * 24, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(int z, int x, int y, CancellationToken cancellationToken)
    {
        if (z is < 0 or > 19) return BadRequest();

        var maxTile = 1 << z;
        if (x < 0 || y < 0 || x >= maxTile || y >= maxTile) return BadRequest();

        var client = _httpClientFactory.CreateClient("MapTiles");
        var osmUrl = $"https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        try
        {
            using var response = await client.GetAsync(osmUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return StatusCode(StatusCodes.Status502BadGateway);

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Response.Headers.CacheControl = "public,max-age=86400";
            return File(bytes, "image/png");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Map tile could not be loaded from {Url}", osmUrl);
            return StatusCode(StatusCodes.Status502BadGateway);
        }
    }
}
