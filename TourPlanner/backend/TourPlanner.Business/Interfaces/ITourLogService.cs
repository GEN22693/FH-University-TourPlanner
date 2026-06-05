using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Interfaces;

public interface ITourLogService
{
    Task<IEnumerable<TourLogResponseDto>> GetLogsByTourIdAsync(int tourId);

    Task<TourLogResponseDto?> GetLogByIdAsync(int tourId, int logId);

    Task<TourLogResponseDto> CreateLogAsync(int tourId, CreateTourLogDto dto);

    Task<TourLogResponseDto?> UpdateLogAsync(int tourId, int logId, UpdateTourLogDto dto);

    Task<bool> DeleteLogAsync(int tourId, int logId);
}
