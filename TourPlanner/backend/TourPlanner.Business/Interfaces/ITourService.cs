using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Interfaces;

public interface ITourService
{
    Task<IEnumerable<TourResponseDto>> GetAllToursAsync(int userId);

    Task<TourResponseDto?> GetTourByIdAsync(int id, int userId);

    Task<TourResponseDto> CreateTourAsync(CreateTourDto dto, int userId);

    Task<TourResponseDto?> UpdateTourAsync(int id, UpdateTourDto dto, int userId);

    Task<bool> DeleteTourAsync(int id, int userId);
}
