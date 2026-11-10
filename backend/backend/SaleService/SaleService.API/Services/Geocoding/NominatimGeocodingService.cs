using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SalesService.Application.Interfaces.Geocoding;

namespace SalesService.API.Services.Geocoding;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Nominatim requires a User-Agent header or it will block requests
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CHB-CFMS-App (admin@chb.com)");
    }

    public async Task<CoordinateDto?> GeocodeAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var result = await FetchCoordinatesAsync(address);

        // Fallback: Nếu không tìm thấy và địa chỉ có số nhà (dấu /), thử bỏ số nhà
        if (result == null && address.Contains('/'))
        {
            _logger.LogInformation("Full address search failed. Attempting fallback for: {Address}", address);

            // Tìm khoảng trắng cuối cùng trước dấu / để cắt bỏ số nhà
            int slashIndex = address.IndexOf('/');
            int spaceBeforeSlash = address.LastIndexOf(' ', slashIndex);

            if (spaceBeforeSlash >= 0)
            {
                string fallbackAddress = address.Substring(spaceBeforeSlash).Trim();
                _logger.LogInformation("Trying fallback address: {FallbackAddress}", fallbackAddress);
                result = await FetchCoordinatesAsync(fallbackAddress);
            }
        }

        if (result != null)
        {
            _logger.LogInformation("Successfully geocoded. Lat={Lat}, Lon={Lon}", result.Latitude, result.Longitude);
        }
        else
        {
            _logger.LogWarning("Geocoding failed for address: {Address}. No results found.", address);
        }

        return result;
    }

    private async Task<CoordinateDto?> FetchCoordinatesAsync(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            var response = await _httpClient.GetFromJsonAsync<List<NominatimSearchResult>>(url);

            if (response != null && response.Any())
            {
                var result = response.First();
                return new CoordinateDto
                {
                    Latitude = double.Parse(result.Lat, System.Globalization.CultureInfo.InvariantCulture),
                    Longitude = double.Parse(result.Lon, System.Globalization.CultureInfo.InvariantCulture)
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling Nominatim Geocoding API for address: {Address}", address);
            return null;
        }
    }

    private class NominatimSearchResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = default!;

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = default!;
    }
}
