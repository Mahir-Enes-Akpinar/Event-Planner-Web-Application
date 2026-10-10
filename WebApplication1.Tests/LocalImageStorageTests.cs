using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests;

public sealed class LocalImageStorageTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "EventPlannerTests", Guid.NewGuid().ToString("N"));
    private readonly LocalImageStorage _storage;

    public LocalImageStorageTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:ImagePath"] = _directory })
            .Build();
        _storage = new LocalImageStorage(new FakeEnvironment(_directory), configuration);
    }

    [Fact]
    public async Task ValidJpeg_IsStoredWithServerGeneratedName()
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4 };
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "image", "attacker-name.jpg");

        var url = await _storage.StoreAsync(file, 1024);

        Assert.StartsWith("/media/", url);
        Assert.DoesNotContain("attacker-name", url);
        Assert.True(_storage.TryResolve(Path.GetFileName(url), out var path, out var type));
        Assert.True(File.Exists(path));
        Assert.Equal("image/jpeg", type);
    }

    [Fact]
    public async Task HtmlDisguisedAsImage_IsRejected()
    {
        var bytes = "<html>not an image</html>"u8.ToArray();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "image", "attack.jpg");

        await Assert.ThrowsAsync<InvalidDataException>(() => _storage.StoreAsync(file, 1024));
    }

    [Fact]
    public void TraversalFileName_IsRejected() =>
        Assert.False(_storage.TryResolve("../secret.jpg", out _, out _));

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    private sealed class FakeEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
