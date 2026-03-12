using Microsoft.Extensions.Configuration;
using SalesService.Application.Interfaces;
using System.Text.Json;

namespace SalesService.API.Services;

public class DistanceService : IDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly string _osrmBaseUrl;

    public DistanceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _osrmBaseUrl = configuration["OSRM_URL"] ?? "http://router.project-osrm.org";
    }

    public async Task<double> CalculateDistanceAsync(double startLat, double startLong, double endLat, double endLong)
    {
        try
        {
            // OSRM format: lon,lat;lon,lat
            var url = $"{_osrmBaseUrl}/route/v1/driving/{startLong},{startLat};{endLong},{endLat}?overview=false";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
                {
                    // distance is in meters
                    var distanceMeters = routes[0].GetProperty("distance").GetDouble();
                    return distanceMeters / 1000.0; // convert to km
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling OSRM: {ex.Message}. Falling back to Haversine.");
        }

        // Fallback to Haversine formula (Bird-fly distance)
        return CalculateHaversineDistance(startLat, startLong, endLat, endLong);
    }

    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double angle) => Math.PI * angle / 180.0;
}
