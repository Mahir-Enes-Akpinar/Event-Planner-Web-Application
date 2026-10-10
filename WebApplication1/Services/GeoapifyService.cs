using System.Text.Json;
using System.Globalization;

namespace WebApplication1.Services;

public sealed class GeoapifyService : IGeoService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public GeoapifyService(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<LocationResult?> ReverseAsync(
        double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var results = await RequestGeoapifyAsync(
            $"v1/geocode/reverse?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&limit=1",
            cancellationToken);
        if (results.Count == 0)
        {
            results = await ReverseNominatimAsync(latitude, longitude, cancellationToken);
        }

        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<LocationResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = await RequestGeoapifyAsync($"v1/geocode/search?text={Uri.EscapeDataString(query)}&filter=countrycode:tr&lang=tr&limit=5", cancellationToken);
        return results.Count > 0
            ? results
            : await SearchNominatimAsync(query, cancellationToken);
    }

    private async Task<IReadOnlyList<LocationResult>> RequestGeoapifyAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Geoapify:ServerApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return Array.Empty<LocationResult>();

        using var response = await _client.GetAsync($"{relativeUrl}&apiKey={Uri.EscapeDataString(apiKey)}", cancellationToken);
        if (!response.IsSuccessStatusCode) return Array.Empty<LocationResult>();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = new List<LocationResult>();
        if (!json.RootElement.TryGetProperty("features", out var features)) return results;
        foreach (var feature in features.EnumerateArray())
        {
            var properties = feature.GetProperty("properties");
            var name = properties.TryGetProperty("formatted", out var formatted)
                ? formatted.GetString()
                : null;
            if (name is null) continue;
            var city = ReadFirstString(properties, "city", "town", "village", "municipality", "state");
            var district = ReadFirstString(properties, "county", "district", "city_district");
            var neighborhood = ReadFirstString(properties, "suburb", "neighbourhood", "neighborhood", "quarter");
            var street = ReadFirstString(properties, "street", "name");

            results.Add(new LocationResult(
                name,
                properties.GetProperty("lat").GetDouble(),
                properties.GetProperty("lon").GetDouble(),
                city,
                district,
                neighborhood,
                street));
        }
        return results;
    }

    private async Task<IReadOnlyList<LocationResult>> SearchNominatimAsync(string query, CancellationToken cancellationToken)
    {
        var url = "https://nominatim.openstreetmap.org/search" +
            $"?format=jsonv2&addressdetails=1&countrycodes=tr&limit=5&q={Uri.EscapeDataString(query)}";
        using var response = await _client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode) return Array.Empty<LocationResult>();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (json.RootElement.ValueKind != JsonValueKind.Array) return Array.Empty<LocationResult>();

        var results = new List<LocationResult>();
        foreach (var item in json.RootElement.EnumerateArray())
        {
            var result = ParseNominatimResult(item);
            if (result is not null) results.Add(result);
        }

        return results;
    }

    private async Task<IReadOnlyList<LocationResult>> ReverseNominatimAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var url = "https://nominatim.openstreetmap.org/reverse" +
            $"?format=jsonv2&addressdetails=1&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}";
        using var response = await _client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode) return Array.Empty<LocationResult>();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var result = ParseNominatimResult(json.RootElement);
        return result is null ? Array.Empty<LocationResult>() : new[] { result };
    }

    private static LocationResult? ParseNominatimResult(JsonElement item)
    {
        if (!item.TryGetProperty("display_name", out var displayNameElement)) return null;
        var displayName = displayNameElement.GetString();
        if (string.IsNullOrWhiteSpace(displayName)) return null;

        var latitude = ReadDouble(item, "lat");
        var longitude = ReadDouble(item, "lon");
        if (latitude is null || longitude is null) return null;

        item.TryGetProperty("address", out var address);
        var city = address.ValueKind == JsonValueKind.Object
            ? ReadFirstString(address, "city", "town", "village", "municipality", "province", "state")
            : null;
        var district = address.ValueKind == JsonValueKind.Object
            ? ReadFirstString(address, "county", "state_district", "district", "city_district")
            : null;
        var neighborhood = address.ValueKind == JsonValueKind.Object
            ? ReadFirstString(address, "suburb", "neighbourhood", "neighborhood", "quarter")
            : null;
        var street = address.ValueKind == JsonValueKind.Object
            ? ReadFirstString(address, "road", "pedestrian", "footway", "street")
            : null;

        return new LocationResult(displayName, latitude.Value, longitude.Value, city, district, neighborhood, street);
    }

    private static double? ReadDouble(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)) return number;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return number;
        return null;
    }

    private static string? ReadFirstString(JsonElement properties, params string[] names)
    {
        foreach (var name in names)
        {
            if (properties.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
        }

        return null;
    }
}
