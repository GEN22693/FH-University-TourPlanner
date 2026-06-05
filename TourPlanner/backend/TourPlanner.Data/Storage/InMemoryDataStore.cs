using TourPlanner.Models;

namespace TourPlanner.Data.Storage;

public class InMemoryDataStore
{
    public List<Tour> Tours { get; } = [];

    public List<User> Users { get; } = [];
}
