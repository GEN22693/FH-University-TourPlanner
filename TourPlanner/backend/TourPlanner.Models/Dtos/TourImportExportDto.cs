using TourPlanner.Models.Enums;

namespace TourPlanner.Models.Dtos;

public class TourImportExportDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public TransportType TransportType { get; set; }

    public double Distance { get; set; }

    public TimeSpan EstimatedTime { get; set; }

    public string RouteInformation { get; set; } = string.Empty;

    public int Popularity { get; set; }

    public string ChildFriendliness { get; set; } = "Unknown";

    public List<TourLogImportExportDto> Logs { get; set; } = [];
}

public class TourLogImportExportDto
{
    public DateTime DateTime { get; set; }

    public string Comment { get; set; } = string.Empty;

    public Difficulty Difficulty { get; set; }

    public double TotalDistance { get; set; }

    public TimeSpan TotalTime { get; set; }

    public int Rating { get; set; }
}