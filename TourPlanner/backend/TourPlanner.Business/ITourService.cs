using TourPlanner.Models;

namespace TourPlanner.Business;

public interface ITourService
{
    IEnumerable<Tour> GetAllTours();
}
