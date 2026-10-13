using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class LocalImageStorage : IImageStorage
{
    private static readonly IReadOnlyDictionary<string, (string Extension, string ContentType)> Signatures =
        new Dictionary<string, (string, string)>
        {
            ["FFD8FF"] = (".jpg", "image/jpeg"),
            ["89504E470D0A1A0A"] = (".png", "image/png"),
            ["52494646"] = (".webp", "image/webp")
        };

    private readonly string _storageRoot;
    private readonly Context? _database;

    public LocalImageStorage(IWebHostEnvironment environment, IConfiguration configuration, Context? database = null)
    {
        _database = database;
        var configuredPath = configuration["Storage:ImagePath"];
        _storageRoot = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "data", "uploads")
            : configuredPath;
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> StoreAsync(
        IFormFile file, long maximumBytes, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0 || file.Length > maximumBytes)
            throw new InvalidDataException($"Görsel boyutu 1 byte ile {maximumBytes / 1024 / 1024} MB arasında olmalıdır.");

        var header = new byte[Math.Min(12, (int)file.Length)];
        await using (var input = file.OpenReadStream())
        {
            var read = await input.ReadAsync(header, cancellationToken);
            if (read < 3) throw new InvalidDataException("Görsel dosyası geçersiz.");
        }

        var hex = Convert.ToHexString(header);
        var signature = Signatures.FirstOrDefault(item => hex.StartsWith(item.Key, StringComparison.Ordinal));
        var isWebP = signature.Key == "52494646" && hex.Length >= 24 && hex.Substring(16, 8) == "57454250";
        if (string.IsNullOrEmpty(signature.Key) || (signature.Key == "52494646" && !isWebP))
            throw new InvalidDataException("Yalnızca doğrulanmış JPEG, PNG veya WebP görselleri kabul edilir.");

        var fileName = $"{Guid.NewGuid():N}{signature.Value.Extension}";
        var target = Path.Combine(_storageRoot, fileName);
        await using var memory = new MemoryStream((int)file.Length);
        await file.CopyToAsync(memory, cancellationToken);
        var content = memory.ToArray();

        await File.WriteAllBytesAsync(target, content, cancellationToken);

        _database?.StoredImages.Add(new StoredImage
        {
            FileName = fileName,
            ContentType = signature.Value.ContentType,
            Content = content,
            CreatedAtUtc = DateTime.UtcNow
        });

        return $"/media/{fileName}";
    }

    public bool TryResolve(string fileName, out string path, out string contentType)
    {
        path = string.Empty;
        contentType = string.Empty;
        if (Path.GetFileName(fileName) != fileName || fileName.Length > 50) return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => string.Empty
        };
        if (contentType.Length == 0) return false;

        path = Path.Combine(_storageRoot, fileName);
        if (File.Exists(path)) return true;

        if (_database is null) return false;

        var storedImage = _database.StoredImages
            .AsNoTracking()
            .FirstOrDefault(item => item.FileName == fileName);
        if (storedImage is null) return false;

        Directory.CreateDirectory(_storageRoot);
        File.WriteAllBytes(path, storedImage.Content);
        contentType = storedImage.ContentType;
        return File.Exists(path);
    }
}
