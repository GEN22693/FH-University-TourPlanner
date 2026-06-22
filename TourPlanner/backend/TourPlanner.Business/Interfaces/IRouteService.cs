using TourPlanner.Models.Enums;

namespace TourPlanner.Business.Interfaces;

public interface IRouteService
{
    Task<RouteResult> GetRouteAsync(string from, string to, TransportType transportType);
}

public class RouteResult
{
    public double DistanceMeters { get; set; }

    public TimeSpan Duration { get; set; }

    public string RouteInformation { get; set; } = string.Empty;
}
