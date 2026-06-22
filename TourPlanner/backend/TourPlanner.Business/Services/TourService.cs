using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Services;

public class TourService : ITourService
{
    private readonly ITourRepository tourRepository;
    private readonly IRouteService routeService;

    public TourService(ITourRepository tourRepository, IRouteService routeService)
    {
        this.tourRepository = tourRepository;
        this.routeService = routeService;
    }

    public async Task<IEnumerable<TourResponseDto>> GetAllToursAsync(int userId)
    {
        IEnumerable<Tour> tours = await tourRepository.GetAllAsync(userId);
        return tours.Select(MapToResponseDto);
    }

    public async Task<TourResponseDto?> GetTourByIdAsync(int id, int userId)
    {
        Tour? tour = await tourRepository.GetByIdAsync(id, userId);
        return tour is null ? null : MapToResponseDto(tour);
    }

    public async Task<TourResponseDto> CreateTourAsync(CreateTourDto dto, int userId)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        RouteResult route = await routeService.GetRouteAsync(dto.From, dto.To, dto.TransportType);

        Tour tour = new()
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            From = dto.From,
            To = dto.To,
            TransportType = dto.TransportType,
            Distance = route.DistanceMeters,
            EstimatedTime = route.Duration,
            RouteInformation = route.RouteInformation,
            CreatedAt = DateTime.UtcNow
        };

        Tour createdTour = await tourRepository.CreateAsync(tour);

        return MapToResponseDto(createdTour);
    }

    public async Task<TourResponseDto?> UpdateTourAsync(int id, UpdateTourDto dto, int userId)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        Tour? tour = await tourRepository.GetByIdAsync(id, userId);
        if (tour is null)
        {
            return null;
        }

        RouteResult route = await routeService.GetRouteAsync(dto.From, dto.To, dto.TransportType);

        tour.Name = dto.Name;
        tour.Description = dto.Description;
        tour.From = dto.From;
        tour.To = dto.To;
        tour.TransportType = dto.TransportType;
        tour.Distance = route.DistanceMeters;
        tour.EstimatedTime = route.Duration;
        tour.RouteInformation = route.RouteInformation;

        Tour? updatedTour = await tourRepository.UpdateAsync(tour, userId);
        return updatedTour is null ? null : MapToResponseDto(updatedTour);
    }

    public Task<bool> DeleteTourAsync(int id, int userId)
    {
        return tourRepository.DeleteAsync(id, userId);
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
