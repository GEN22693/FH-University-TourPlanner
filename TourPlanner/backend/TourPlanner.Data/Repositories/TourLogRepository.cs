using Microsoft.EntityFrameworkCore;
using TourPlanner.Data.Context;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.Data.Repositories;

public class TourLogRepository : ITourLogRepository
{
    private readonly AppDbContext dbContext;

    public TourLogRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<TourLog>> GetByTourIdAsync(int tourId)
    {
        return await dbContext.TourLogs
            .AsNoTracking()
            .Where(log => log.TourId == tourId)
            .ToListAsync();
    }

    public Task<TourLog?> GetByIdAsync(int tourId, int logId)
    {
        return dbContext.TourLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(log => log.TourId == tourId && log.Id == logId);
    }

    public async Task<TourLog> CreateAsync(TourLog tourLog)
    {
        dbContext.TourLogs.Add(tourLog);
        await dbContext.SaveChangesAsync();

        return tourLog;
    }

    public async Task<TourLog?> UpdateAsync(TourLog tourLog)
    {
        TourLog? existingLog = await dbContext.TourLogs
            .FirstOrDefaultAsync(log => log.TourId == tourLog.TourId && log.Id == tourLog.Id);

        if (existingLog is null)
        {
            return null;
        }

        existingLog.DateTime = tourLog.DateTime;
        existingLog.Comment = tourLog.Comment;
        existingLog.Difficulty = tourLog.Difficulty;
        existingLog.TotalDistance = tourLog.TotalDistance;
        existingLog.TotalTime = tourLog.TotalTime;
        existingLog.Rating = tourLog.Rating;

        await dbContext.SaveChangesAsync();

        return existingLog;
    }

    public async Task<bool> DeleteAsync(int tourId, int logId)
    {
        TourLog? tourLog = await dbContext.TourLogs
            .FirstOrDefaultAsync(log => log.TourId == tourId && log.Id == logId);

        if (tourLog is null)
        {
            return false;
        }

        dbContext.TourLogs.Remove(tourLog);
        await dbContext.SaveChangesAsync();

        return true;
    }
}
