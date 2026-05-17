using TourPlanner.Models.Enums;

namespace TourPlanner.Models.Dtos;

public class UpdateTourDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public TransportType TransportType { get; set; }
}
