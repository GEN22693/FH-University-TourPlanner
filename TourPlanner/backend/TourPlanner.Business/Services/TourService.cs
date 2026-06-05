using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Services;

public class TourService : ITourService
{
    private const int TemporarySystemUserId = 1;
    private readonly ITourRepository tourRepository;

    public TourService(ITourRepository tourRepository)
    {
        this.tourRepository = tourRepository;
    }

    public async Task<IEnumerable<TourResponseDto>> GetAllToursAsync()
    {
        IEnumerable<Tour> tours = await tourRepository.GetAllAsync();
        return tours.Select(MapToResponseDto);
    }

    public async Task<TourResponseDto?> GetTourByIdAsync(int id)
    {
        Tour? tour = await tourRepository.GetByIdAsync(id);
        return tour is null ? null : MapToResponseDto(tour);
    }

    public async Task<TourResponseDto> CreateTourAsync(CreateTourDto dto)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        Tour tour = new()
        {
            UserId = TemporarySystemUserId,
            Name = dto.Name,
            Description = dto.Description,
            From = dto.From,
            To = dto.To,
            TransportType = dto.TransportType,
            Distance = 0,
            EstimatedTime = TimeSpan.Zero,
            RouteInformation = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        Tour createdTour = await tourRepository.CreateAsync(tour);

        return MapToResponseDto(createdTour);
    }

    public async Task<TourResponseDto?> UpdateTourAsync(int id, UpdateTourDto dto)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        Tour? tour = await tourRepository.GetByIdAsync(id);
        if (tour is null)
        {
            return null;
        }

        tour.Name = dto.Name;
        tour.Description = dto.Description;
        tour.From = dto.From;
        tour.To = dto.To;
        tour.TransportType = dto.TransportType;

        Tour? updatedTour = await tourRepository.UpdateAsync(tour);
        return updatedTour is null ? null : MapToResponseDto(updatedTour);
    }

    public Task<bool> DeleteTourAsync(int id)
    {
        return tourRepository.DeleteAsync(id);
    }

    private static void ValidateTour(string name, string from, string to)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new ArgumentException("From must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("To must not be empty.");
        }

        if (string.Equals(from.Trim(), to.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("From and To must not be identical.");
        }
    }

    private static TourResponseDto MapToResponseDto(Tour tour)
    {
        return new TourResponseDto
        {
            Id = tour.Id,
            Name = tour.Name,
            Description = tour.Description,
            From = tour.From,
            To = tour.To,
            TransportType = tour.TransportType,
            Distance = tour.Distance,
            EstimatedTime = tour.EstimatedTime,
            RouteInformation = tour.RouteInformation
        };
    }
}
