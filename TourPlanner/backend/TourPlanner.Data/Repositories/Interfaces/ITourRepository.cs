using TourPlanner.Models;

namespace TourPlanner.Data.Repositories.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllAsync(int userId);

    Task<Tour?> GetByIdAsync(int id, int userId);

    Task<Tour> CreateAsync(Tour tour);

    Task<Tour?> UpdateAsync(Tour tour, int userId);

    Task<bool> DeleteAsync(int id, int userId);
}
