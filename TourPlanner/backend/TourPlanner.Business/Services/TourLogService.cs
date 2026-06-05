using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;
using TourPlanner.Models.Enums;

namespace TourPlanner.Business.Services;

public class TourLogService : ITourLogService
{
    private readonly ITourRepository tourRepository;
    private readonly ITourLogRepository tourLogRepository;

    public TourLogService(ITourRepository tourRepository, ITourLogRepository tourLogRepository)
    {
        this.tourRepository = tourRepository;
        this.tourLogRepository = tourLogRepository;
    }

    public async Task<IEnumerable<TourLogResponseDto>> GetLogsByTourIdAsync(int tourId)
    {
        Tour? tour = await tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        IEnumerable<TourLog> logs = await tourLogRepository.GetByTourIdAsync(tourId);
        return logs.Select(MapToResponseDto);
    }

    public async Task<TourLogResponseDto?> GetLogByIdAsync(int tourId, int logId)
    {
        Tour? tour = await tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        TourLog? tourLog = await tourLogRepository.GetByIdAsync(tourId, logId);
        return tourLog is null ? null : MapToResponseDto(tourLog);
    }

    public async Task<TourLogResponseDto> CreateLogAsync(int tourId, CreateTourLogDto dto)
    {
        Tour? tour = await tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        ValidateTourLog(dto.Difficulty, dto.TotalDistance, dto.TotalTime, dto.Rating);

        TourLog tourLog = new()
        {
            TourId = tour.Id,
            DateTime = ToUtc(dto.DateTime),
            Comment = dto.Comment,
            Difficulty = dto.Difficulty,
            TotalDistance = dto.TotalDistance,
            TotalTime = dto.TotalTime,
            Rating = dto.Rating
        };

        TourLog createdLog = await tourLogRepository.CreateAsync(tourLog);

        return MapToResponseDto(createdLog);
    }

    public async Task<TourLogResponseDto?> UpdateLogAsync(int tourId, int logId, UpdateTourLogDto dto)
    {
        Tour? tour = await tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        ValidateTourLog(dto.Difficulty, dto.TotalDistance, dto.TotalTime, dto.Rating);

        TourLog? tourLog = await tourLogRepository.GetByIdAsync(tourId, logId);
        if (tourLog is null)
        {
            return null;
        }

        tourLog.DateTime = ToUtc(dto.DateTime);
        tourLog.Comment = dto.Comment;
        tourLog.Difficulty = dto.Difficulty;
        tourLog.TotalDistance = dto.TotalDistance;
        tourLog.TotalTime = dto.TotalTime;
        tourLog.Rating = dto.Rating;

        TourLog? updatedLog = await tourLogRepository.UpdateAsync(tourLog);
        return updatedLog is null ? null : MapToResponseDto(updatedLog);
    }

    public async Task<bool> DeleteLogAsync(int tourId, int logId)
    {
        Tour? tour = await tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new ArgumentException("Tour does not exist.");
        }

        return await tourLogRepository.DeleteAsync(tourId, logId);
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

    private static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
