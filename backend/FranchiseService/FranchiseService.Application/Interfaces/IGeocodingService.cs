namespace FranchiseService.Application.Interfaces;

public interface IGeocodingService
{
    Task<CoordinateDto?> GeocodeAsync(string address);
}

public class CoordinateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
