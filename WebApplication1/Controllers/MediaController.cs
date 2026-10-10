using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[AllowAnonymous]
public sealed class MediaController : Controller
{
    private readonly IImageStorage _storage;
    public MediaController(IImageStorage storage) => _storage = storage;

    [HttpGet("media/{fileName}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult Get(string fileName)
    {
        if (!_storage.TryResolve(fileName, out var path, out var contentType)) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }
}
