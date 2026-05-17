using TourPlanner.Business.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Services;

public class TourService : ITourService
{
    private readonly List<Tour> tours = [];
    private int nextId = 1;

    public Task<IEnumerable<TourResponseDto>> GetAllToursAsync()
    {
        return Task.FromResult(tours.Select(MapToResponseDto));
    }

    public Task<TourResponseDto?> GetTourByIdAsync(int id)
    {
        Tour? tour = tours.FirstOrDefault(tour => tour.Id == id);
        return Task.FromResult(tour is null ? null : MapToResponseDto(tour));
    }

    public Task<TourResponseDto> CreateTourAsync(CreateTourDto dto)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        Tour tour = new()
        {
            Id = nextId++,
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

        tours.Add(tour);

        return Task.FromResult(MapToResponseDto(tour));
    }

    public Task<TourResponseDto?> UpdateTourAsync(int id, UpdateTourDto dto)
    {
        ValidateTour(dto.Name, dto.From, dto.To);

        Tour? tour = tours.FirstOrDefault(tour => tour.Id == id);
        if (tour is null)
        {
            return Task.FromResult<TourResponseDto?>(null);
        }

        tour.Name = dto.Name;
        tour.Description = dto.Description;
        tour.From = dto.From;
        tour.To = dto.To;
        tour.TransportType = dto.TransportType;

        return Task.FromResult<TourResponseDto?>(MapToResponseDto(tour));
    }

    public Task<bool> DeleteTourAsync(int id)
    {
        Tour? tour = tours.FirstOrDefault(tour => tour.Id == id);
        if (tour is null)
        {
            return Task.FromResult(false);
        }

        tours.Remove(tour);
        return Task.FromResult(true);
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
