using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Interfaces;

public interface ITourService
{
    Task<IEnumerable<TourResponseDto>> GetAllToursAsync();

    Task<TourResponseDto?> GetTourByIdAsync(int id);

    Task<TourResponseDto> CreateTourAsync(CreateTourDto dto);

    Task<TourResponseDto?> UpdateTourAsync(int id, UpdateTourDto dto);

    Task<bool> DeleteTourAsync(int id);
}
