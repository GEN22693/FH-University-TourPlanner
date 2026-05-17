using TourPlanner.Models.Enums;

namespace TourPlanner.Models.Dtos;

public class TourResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public TransportType TransportType { get; set; }

    public double Distance { get; set; }

    public TimeSpan EstimatedTime { get; set; }

    public string RouteInformation { get; set; } = string.Empty;
}
