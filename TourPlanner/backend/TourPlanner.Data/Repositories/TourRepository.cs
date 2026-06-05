using Microsoft.EntityFrameworkCore;
using TourPlanner.Data.Context;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.Data.Repositories;

public class TourRepository : ITourRepository
{
    private readonly AppDbContext dbContext;

    public TourRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<Tour>> GetAllAsync()
    {
        return await dbContext.Tours
            .AsNoTracking()
            .Include(tour => tour.TourLogs)
            .ToListAsync();
    }

    public Task<Tour?> GetByIdAsync(int id)
    {
        return dbContext.Tours
            .AsNoTracking()
            .Include(tour => tour.TourLogs)
            .FirstOrDefaultAsync(tour => tour.Id == id);
    }

    public async Task<Tour> CreateAsync(Tour tour)
    {
        dbContext.Tours.Add(tour);
        await dbContext.SaveChangesAsync();

        return tour;
    }

    public async Task<Tour?> UpdateAsync(Tour tour)
    {
        Tour? existingTour = await dbContext.Tours.FirstOrDefaultAsync(existingTour => existingTour.Id == tour.Id);
        if (existingTour is null)
        {
            return null;
        }

        existingTour.Name = tour.Name;
        existingTour.Description = tour.Description;
        existingTour.From = tour.From;
        existingTour.To = tour.To;
        existingTour.TransportType = tour.TransportType;
        existingTour.Distance = tour.Distance;
        existingTour.EstimatedTime = tour.EstimatedTime;
        existingTour.RouteInformation = tour.RouteInformation;

        await dbContext.SaveChangesAsync();

        return existingTour;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Tour? tour = await dbContext.Tours.FirstOrDefaultAsync(tour => tour.Id == id);
        if (tour is null)
        {
            return false;
        }

        dbContext.Tours.Remove(tour);
        await dbContext.SaveChangesAsync();

        return true;
    }
}
