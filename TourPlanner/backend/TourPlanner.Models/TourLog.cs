namespace TourPlanner.Models;

public class TourLog
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Comment { get; set; } = string.Empty;
}
