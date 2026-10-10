namespace WebApplication1.Services;

public interface IImageStorage
{
    Task<string> StoreAsync(IFormFile file, long maximumBytes, CancellationToken cancellationToken = default);
    bool TryResolve(string fileName, out string path, out string contentType);
}
