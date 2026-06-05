using TourPlanner.Models;

namespace TourPlanner.Data.Repositories.Interfaces;

public interface ITourRepository
{
    Task<IEnumerable<Tour>> GetAllAsync();

    Task<Tour?> GetByIdAsync(int id);

    Task<Tour> CreateAsync(Tour tour);

    Task<Tour?> UpdateAsync(Tour tour);

    Task<bool> DeleteAsync(int id);
}
