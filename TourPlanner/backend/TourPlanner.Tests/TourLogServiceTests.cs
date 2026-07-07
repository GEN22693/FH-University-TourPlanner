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
    private Mock<ITourRepository> tourRepositoryMock = null!;
    private Mock<ITourLogRepository> tourLogRepositoryMock = null!;
    private TourLogService tourLogService = null!;

    private static readonly Tour ExistingTour = new() { Id = 1, UserId = 1, Name = "Test Tour", From = "Vienna", To = "Graz" };

    [SetUp]
    public void SetUp()
    {
        tourRepositoryMock = new Mock<ITourRepository>();
        tourLogRepositoryMock = new Mock<ITourLogRepository>();
        tourLogService = new TourLogService(tourRepositoryMock.Object, tourLogRepositoryMock.Object);
    }

    [Test]
    public async Task CreateLogAsync_CreatesAndReturnsLog_WhenTourBelongsToUser()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(ExistingTour);

        var createDto = new CreateTourLogDto
        {
            DateTime = new DateTime(2026, 6, 1, 14, 0, 0, DateTimeKind.Utc),
            Comment = "Amazing hike with great views",
            Difficulty = Difficulty.Hard,
            TotalDistance = 25.5,
            TotalTime = TimeSpan.FromHours(4),
            Rating = 5
        };

        var savedLog = new TourLog
        {
            Id = 100,
            TourId = 1,
            DateTime = createDto.DateTime,
            Comment = createDto.Comment,
            Difficulty = Difficulty.Hard,
            TotalDistance = 25.5,
            TotalTime = TimeSpan.FromHours(4),
            Rating = 5
        };

        tourLogRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TourLog>()))
            .ReturnsAsync(savedLog);

        var result = await tourLogService.CreateLogAsync(1, createDto, 1);

        Assert.That(result.Id, Is.EqualTo(100));
        Assert.That(result.Rating, Is.EqualTo(5));
        Assert.That(result.TotalDistance, Is.EqualTo(25.5));
        Assert.That(result.Comment, Is.EqualTo("Amazing hike with great views"));

        tourRepositoryMock.Verify(r => r.GetByIdAsync(1, 1), Times.Once);

        tourLogRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<TourLog>(log =>
                log.TourId == 1 &&
                log.Rating == 5 &&
                log.Difficulty == Difficulty.Hard &&
                log.TotalDistance == 25.5)),
            Times.Once);
    }

    [Test]
    public void CreateLogAsync_ThrowsArgumentException_WhenTourNotFoundForUser()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 99))
            .ReturnsAsync((Tour?)null);

        var createDto = new CreateTourLogDto
        {
            DateTime = DateTime.UtcNow,
            Difficulty = Difficulty.Easy,
            TotalDistance = 10,
            TotalTime = TimeSpan.FromHours(1),
            Rating = 3
        };

        Assert.ThrowsAsync<ArgumentException>(() => tourLogService.CreateLogAsync(1, createDto, 99));

        tourLogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<TourLog>()), Times.Never);
    }

    [Test]
    public async Task GetLogsByTourIdAsync_ReturnsAllLogs_WhenTourBelongsToUser()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(ExistingTour);

        var logsFromDb = new[]
        {
            new TourLog { Id = 10, TourId = 1, Rating = 5, Comment = "Excellent" },
            new TourLog { Id = 11, TourId = 1, Rating = 4, Comment = "Good" }
        };

        tourLogRepositoryMock.Setup(r => r.GetByTourIdAsync(1))
            .ReturnsAsync(logsFromDb);

        var result = await tourLogService.GetLogsByTourIdAsync(1, 1);

        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList[0].Rating, Is.EqualTo(5));
        Assert.That(resultList[1].Comment, Is.EqualTo("Good"));
    }

    [Test]
    public async Task DeleteLogAsync_ReturnsTrue_WhenLogIsDeleted()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(ExistingTour);

        tourLogRepositoryMock.Setup(r => r.DeleteAsync(1, 10))
            .ReturnsAsync(true);

        var result = await tourLogService.DeleteLogAsync(1, 10, 1);

        Assert.That(result, Is.True);

        tourRepositoryMock.Verify(r => r.GetByIdAsync(1, 1), Times.Once);

        tourLogRepositoryMock.Verify(r => r.DeleteAsync(1, 10), Times.Once);
    }

    [Test]
    public void UpdateLogAsync_ThrowsArgumentException_WhenTourNotFoundForUser()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 99))
            .ReturnsAsync((Tour?)null);

        var updateDto = new UpdateTourLogDto
        {
            DateTime = DateTime.UtcNow,
            Difficulty = Difficulty.Easy,
            TotalDistance = 10,
            TotalTime = TimeSpan.FromHours(1),
            Rating = 4
        };

        Assert.ThrowsAsync<ArgumentException>(() => tourLogService.UpdateLogAsync(1, 10, updateDto, 99));

        tourLogRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TourLog>()), Times.Never);
    }

    [Test]
    public async Task GetLogByIdAsync_ReturnsDtoWithCorrectData_WhenLogExists()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(ExistingTour);

        var logFromDb = new TourLog
        {
            Id = 15,
            TourId = 1,
            Rating = 4,
            Difficulty = Difficulty.Medium,
            TotalDistance = 20,
            TotalTime = TimeSpan.FromHours(3),
            Comment = "Fun and challenging"
        };

        tourLogRepositoryMock.Setup(r => r.GetByIdAsync(1, 15))
            .ReturnsAsync(logFromDb);

        var result = await tourLogService.GetLogByIdAsync(1, 15, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(15));
        Assert.That(result.Rating, Is.EqualTo(4));
        Assert.That(result.Comment, Is.EqualTo("Fun and challenging"));
        Assert.That(result.Difficulty, Is.EqualTo(Difficulty.Medium));
    }

    [Test]
    public async Task CreateLogAsync_ConvertsDateTimeToUtc()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(ExistingTour);

        var localDateTime = new DateTime(2026, 6, 1, 14, 0, 0, DateTimeKind.Local);
        var createDto = new CreateTourLogDto
        {
            DateTime = localDateTime,
            Difficulty = Difficulty.Easy,
            TotalDistance = 10,
            TotalTime = TimeSpan.FromHours(1),
            Rating = 3
        };

        TourLog? capturedLog = null;
        tourLogRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<TourLog>()))
            .Callback<TourLog>(log => capturedLog = log)
            .ReturnsAsync(new TourLog { Id = 1, TourId = 1 });

        await tourLogService.CreateLogAsync(1, createDto, 1);

        Assert.That(capturedLog, Is.Not.Null);
        Assert.That(capturedLog!.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }
}
