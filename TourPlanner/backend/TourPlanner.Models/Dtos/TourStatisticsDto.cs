namespace TourPlanner.Models.Dtos;

public class TourStatisticsDto
{
    public int TotalTours { get; set; }

    public int TotalLogs { get; set; }

    public double TotalDistance { get; set; }

    public TimeSpan TotalEstimatedTime { get; set; }

    public double AverageRating { get; set; }

    public string MostPopularTourName { get; set; } = string.Empty;

    public string BestRatedTourName { get; set; } = string.Empty;
}