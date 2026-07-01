using Moq;
using TourPlanner.Business.Services;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;
using TourPlanner.Models.Enums;

namespace TourPlanner.Tests;

[TestFixture]
public class TourLogServiceTests
{
    private static readonly Tour ExistingTour = new() { Id = 1, UserId = 1, Name = "Test Tour", From = "Vienna", To = "Graz" };

    private static CreateTourLogDto ValidDto() => new()
    {
        DateTime = DateTime.UtcNow,
        Difficulty = Difficulty.Easy,
        TotalDistance = 10,
        TotalTime = TimeSpan.FromHours(1),
        Rating = 3
    };

    // --- Validierung (nur Tour-Repo Mock nötig, kein Log-Repo) ---

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenTourNotFound()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(99, 1)).ReturnsAsync((Tour?)null);

        TourLogService service = new(repoMock.Object, null!);
        Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogAsync(99, ValidDto(), 1));
    }

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenRatingTooLow()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);

        TourLogService service = new(repoMock.Object, null!);
        CreateTourLogDto dto = ValidDto();
        dto.Rating = 0;

        Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogAsync(1, dto, 1));
    }

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenRatingTooHigh()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);

        TourLogService service = new(repoMock.Object, null!);
        CreateTourLogDto dto = ValidDto();
        dto.Rating = 6;

        Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogAsync(1, dto, 1));
    }

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenTotalDistanceIsNegative()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);

        TourLogService service = new(repoMock.Object, null!);
        CreateTourLogDto dto = ValidDto();
        dto.TotalDistance = -1;

        Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogAsync(1, dto, 1));
    }

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenTotalTimeIsZero()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);

        TourLogService service = new(repoMock.Object, null!);
        CreateTourLogDto dto = ValidDto();
        dto.TotalTime = TimeSpan.Zero;

        Assert.ThrowsAsync<ArgumentException>(() => service.CreateLogAsync(1, dto, 1));
    }

    //  Happy Path (beide Repos gebraucht)

    [Test]
    public async Task CreateLogAsync_CreatesLog_WhenTourExists()
    {
        Mock<ITourRepository> repoMock = new();
        Mock<ITourLogRepository> logRepoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);
        logRepoMock.Setup(r => r.CreateAsync(It.IsAny<TourLog>())).ReturnsAsync(
            new TourLog { Id = 7, TourId = 1, Rating = 3, Difficulty = Difficulty.Easy, TotalDistance = 10, TotalTime = TimeSpan.FromHours(1) });

        TourLogService service = new(repoMock.Object, logRepoMock.Object);
        TourLogResponseDto result = await service.CreateLogAsync(1, ValidDto(), 1);

        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.TourId, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteLogAsync_ReturnsTrue_WhenLogExists()
    {
        Mock<ITourRepository> repoMock = new();
        Mock<ITourLogRepository> logRepoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(ExistingTour);
        logRepoMock.Setup(r => r.DeleteAsync(1, 10)).ReturnsAsync(true);

        TourLogService service = new(repoMock.Object, logRepoMock.Object);
        bool result = await service.DeleteLogAsync(1, 10, 1);

        Assert.That(result, Is.True);
    }
}
