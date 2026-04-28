using TourPlanner.Models;

namespace TourPlanner.Business;

public class TourService : ITourService
{
    public IEnumerable<Tour> GetAllTours()
    {
        return [];
    }
}
