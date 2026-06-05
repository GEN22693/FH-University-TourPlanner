using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Storage;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;
using TourPlanner.Models.Enums;

namespace TourPlanner.Business.Services;

public class TourLogService : ITourLogService
{
    private readonly InMemoryDataStore dataStore;

    public TourLogService(InMemoryDataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    public Task<IEnumerable<TourLogResponseDto>> GetLogsByTourIdAsync(int tourId)
    {
        Tour? tour = FindTour(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        return Task.FromResult(tour.TourLogs.Select(MapToResponseDto));
    }

    public Task<TourLogResponseDto?> GetLogByIdAsync(int tourId, int logId)
    {
        Tour? tour = FindTour(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        TourLog? tourLog = tour.TourLogs.FirstOrDefault(log => log.Id == logId);
        return Task.FromResult(tourLog is null ? null : MapToResponseDto(tourLog));
    }

    public Task<TourLogResponseDto> CreateLogAsync(int tourId, CreateTourLogDto dto)
    {
        Tour? tour = FindTour(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        ValidateTourLog(dto.Difficulty, dto.TotalDistance, dto.TotalTime, dto.Rating);

        TourLog tourLog = new()
        {
            Id = GetNextLogId(tour),
            TourId = tour.Id,
            Tour = tour,
            DateTime = dto.DateTime,
            Comment = dto.Comment,
            Difficulty = dto.Difficulty,
            TotalDistance = dto.TotalDistance,
            TotalTime = dto.TotalTime,
            Rating = dto.Rating
        };

        tour.TourLogs.Add(tourLog);

        return Task.FromResult(MapToResponseDto(tourLog));
    }

    public Task<TourLogResponseDto?> UpdateLogAsync(int tourId, int logId, UpdateTourLogDto dto)
    {
        Tour? tour = FindTour(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        ValidateTourLog(dto.Difficulty, dto.TotalDistance, dto.TotalTime, dto.Rating);

        TourLog? tourLog = tour.TourLogs.FirstOrDefault(log => log.Id == logId);
        if (tourLog is null)
        {
            return Task.FromResult<TourLogResponseDto?>(null);
        }

        tourLog.DateTime = dto.DateTime;
        tourLog.Comment = dto.Comment;
        tourLog.Difficulty = dto.Difficulty;
        tourLog.TotalDistance = dto.TotalDistance;
        tourLog.TotalTime = dto.TotalTime;
        tourLog.Rating = dto.Rating;

        return Task.FromResult<TourLogResponseDto?>(MapToResponseDto(tourLog));
    }

    public Task<bool> DeleteLogAsync(int tourId, int logId)
    {
        Tour? tour = FindTour(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        TourLog? tourLog = tour.TourLogs.FirstOrDefault(log => log.Id == logId);
        if (tourLog is null)
        {
            return Task.FromResult(false);
        }

        tour.TourLogs.Remove(tourLog);
        return Task.FromResult(true);
    }

    private Tour? FindTour(int tourId)
    {
        return dataStore.Tours.FirstOrDefault(tour => tour.Id == tourId);
    }

    private static int GetNextLogId(Tour tour)
    {
        return tour.TourLogs.Count == 0 ? 1 : tour.TourLogs.Max(log => log.Id) + 1;
    }

    private static void ValidateTourLog(Difficulty difficulty, double totalDistance, TimeSpan totalTime, int rating)
    {
        if (rating is < 1 or > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        if (totalDistance < 0)
        {
            throw new ArgumentException("Total distance must not be negative.");
        }

        if (totalTime <= TimeSpan.Zero)
        {
            throw new ArgumentException("Total time must be greater than zero.");
        }

        if (!Enum.IsDefined(difficulty))
        {
            throw new ArgumentException("Difficulty must be Easy, Medium or Hard.");
        }
    }

    private static TourLogResponseDto MapToResponseDto(TourLog tourLog)
    {
        return new TourLogResponseDto
        {
            Id = tourLog.Id,
            TourId = tourLog.TourId,
            DateTime = tourLog.DateTime,
            Comment = tourLog.Comment,
            Difficulty = tourLog.Difficulty,
            TotalDistance = tourLog.TotalDistance,
            TotalTime = tourLog.TotalTime,
            Rating = tourLog.Rating
        };
    }
}
