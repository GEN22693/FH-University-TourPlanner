using TourPlanner.Models.Enums;

namespace TourPlanner.Models.Dtos;

public class UpdateTourLogDto
{
    public DateTime DateTime { get; set; }

    public string Comment { get; set; } = string.Empty;

    public Difficulty Difficulty { get; set; }

    public double TotalDistance { get; set; }

    public TimeSpan TotalTime { get; set; }

    public int Rating { get; set; }
}
