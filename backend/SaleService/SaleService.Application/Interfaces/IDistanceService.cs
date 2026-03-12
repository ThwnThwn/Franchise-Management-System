namespace SalesService.Application.Interfaces;

public interface IDistanceService
{
    /// <summary>
    /// Calculates the distance between two points in kilometers.
    /// </summary>
    Task<double> CalculateDistanceAsync(double startLat, double startLong, double endLat, double endLong);
}
