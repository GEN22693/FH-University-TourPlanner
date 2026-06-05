using Microsoft.EntityFrameworkCore;
using TourPlanner.Models;

namespace TourPlanner.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Tour> Tours => Set<Tour>();

    public DbSet<TourLog> TourLogs => Set<TourLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id)
                .UseIdentityByDefaultColumn()
                .HasIdentityOptions(startValue: 2);

            entity.Property(user => user.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.HasMany(user => user.Tours)
                .WithOne(tour => tour.User)
                .HasForeignKey(tour => tour.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(new User
            {
                Id = 1,
                Username = "system",
                Email = "system@tourplanner.local",
                PasswordHash = "development-system-user",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(tour => tour.Id);

            entity.Property(tour => tour.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(tour => tour.Description)
                .HasMaxLength(2000);

            entity.Property(tour => tour.From)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(tour => tour.To)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(tour => tour.RouteInformation)
                .HasMaxLength(4000);

            entity.Property(tour => tour.CreatedAt)
                .IsRequired();

            entity.HasMany(tour => tour.TourLogs)
                .WithOne(log => log.Tour)
                .HasForeignKey(log => log.TourId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TourLog>(entity =>
        {
            entity.HasKey(log => log.Id);

            entity.Property(log => log.Comment)
                .HasMaxLength(2000);

            entity.Property(log => log.DateTime)
                .IsRequired();
        });
    }
}
