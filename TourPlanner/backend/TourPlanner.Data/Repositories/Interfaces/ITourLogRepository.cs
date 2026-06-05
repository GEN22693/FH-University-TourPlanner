using TourPlanner.Models;

namespace TourPlanner.Data.Repositories.Interfaces;

public interface ITourLogRepository
{
    Task<IEnumerable<TourLog>> GetByTourIdAsync(int tourId);

    Task<TourLog?> GetByIdAsync(int tourId, int logId);

    Task<TourLog> CreateAsync(TourLog tourLog);

    Task<TourLog?> UpdateAsync(TourLog tourLog);

    Task<bool> DeleteAsync(int tourId, int logId);
}
