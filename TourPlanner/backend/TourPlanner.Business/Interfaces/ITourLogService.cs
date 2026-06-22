using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Interfaces;

public interface ITourLogService
{
    Task<IEnumerable<TourLogResponseDto>> GetLogsByTourIdAsync(int tourId, int userId);

    Task<TourLogResponseDto?> GetLogByIdAsync(int tourId, int logId, int userId);

    Task<TourLogResponseDto> CreateLogAsync(int tourId, CreateTourLogDto dto, int userId);

    Task<TourLogResponseDto?> UpdateLogAsync(int tourId, int logId, UpdateTourLogDto dto, int userId);

    Task<bool> DeleteLogAsync(int tourId, int logId, int userId);
}
