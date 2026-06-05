using Microsoft.EntityFrameworkCore;
using TourPlanner.Data.Context;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;

namespace TourPlanner.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        string normalizedEmail = email.ToLower();

        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        string normalizedUsername = username.ToLower();

        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username.ToLower() == normalizedUsername);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        string normalizedEmail = email.ToLower();

        return dbContext.Users
            .AnyAsync(user => user.Email.ToLower() == normalizedEmail);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        string normalizedUsername = username.ToLower();

        return dbContext.Users
            .AnyAsync(user => user.Username.ToLower() == normalizedUsername);
    }

    public async Task<User> CreateAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }
}
