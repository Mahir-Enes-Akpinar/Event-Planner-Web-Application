namespace WebApplication1.Services;

public sealed record LocationResult(
    string DisplayName,
    double Latitude,
    double Longitude,
    string? City = null,
    string? District = null,
    string? Neighborhood = null,
    string? Street = null);

public interface IGeoService
{
    Task<IReadOnlyList<LocationResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<LocationResult?> ReverseAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
