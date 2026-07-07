using Moq;
using TourPlanner.Business.Interfaces;
using TourPlanner.Business.Services;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;
using TourPlanner.Models.Enums;

namespace TourPlanner.Tests;

[TestFixture]
public class TourServiceTests
{
    private Mock<ITourRepository> tourRepositoryMock = null!;
    private Mock<IRouteService> routeServiceMock = null!;
    private TourService tourService = null!;

    [SetUp]
    public void SetUp()
    {
        tourRepositoryMock = new Mock<ITourRepository>();
        routeServiceMock = new Mock<IRouteService>();
        tourService = new TourService(tourRepositoryMock.Object, routeServiceMock.Object);
    }

    [Test]
    public async Task CreateTourAsync_CallsRouteService_AndSavesToRepository()
    {
        var createDto = new CreateTourDto
        {
            Name = "Vienna to Prague",
            From = "Vienna",
            To = "Prague",
            TransportType = TransportType.Vacation,
            Description = "A scenic trip"
        };

        var fakeRoute = new RouteResult
        {
            DistanceMeters = 330000,
            Duration = TimeSpan.FromHours(3.5),
            RouteInformation = "Distance: 330.00 km, Duration: 03:30:00"
        };

        routeServiceMock.Setup(r => r.GetRouteAsync("Vienna", "Prague", TransportType.Vacation))
            .ReturnsAsync(fakeRoute);

        var savedTour = new Tour
        {
            Id = 1,
            UserId = 1,
            Name = "Vienna to Prague",
            Distance = 330000,
            EstimatedTime = TimeSpan.FromHours(3.5),
            RouteInformation = "Distance: 330.00 km, Duration: 03:30:00"
        };

        tourRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
            .ReturnsAsync(savedTour);

        var result = await tourService.CreateTourAsync(createDto, userId: 1);

        Assert.That(result.Distance, Is.EqualTo(330000));
        Assert.That(result.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(3.5)));
        Assert.That(result.RouteInformation, Is.EqualTo("Distance: 330.00 km, Duration: 03:30:00"));

        routeServiceMock.Verify(
            r => r.GetRouteAsync("Vienna", "Prague", TransportType.Vacation),
            Times.Once);

        tourRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Tour>(t =>
                t.Distance == 330000 &&
                t.UserId == 1 &&
                t.Name == "Vienna to Prague")),
            Times.Once);
    }

    [Test]
    public async Task UpdateTourAsync_RecalculatesRoute_WhenFromToChanges()
    {
        var existingTour = new Tour
        {
            Id = 5,
            UserId = 1,
            Name = "Old Route",
            From = "Vienna",
            To = "Graz",
            Distance = 200000,
            EstimatedTime = TimeSpan.FromHours(2)
        };

        tourRepositoryMock.Setup(r => r.GetByIdAsync(5, 1))
            .ReturnsAsync(existingTour);

        var newRoute = new RouteResult
        {
            DistanceMeters = 560000,
            Duration = TimeSpan.FromHours(5.5),
            RouteInformation = "Distance: 560.00 km, Duration: 05:30:00"
        };

        routeServiceMock.Setup(r => r.GetRouteAsync("Vienna", "Munich", TransportType.Vacation))
            .ReturnsAsync(newRoute);

        var updatedTour = new Tour
        {
            Id = 5,
            UserId = 1,
            Name = "Vienna to Munich",
            From = "Vienna",
            To = "Munich",
            Distance = 560000,
            EstimatedTime = TimeSpan.FromHours(5.5),
            RouteInformation = "Distance: 560.00 km, Duration: 05:30:00"
        };

        tourRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), 1))
            .ReturnsAsync(updatedTour);

        var updateDto = new UpdateTourDto
        {
            Name = "Vienna to Munich",
            From = "Vienna",
            To = "Munich",
            TransportType = TransportType.Vacation,
            Description = "Updated trip"
        };

        var result = await tourService.UpdateTourAsync(5, updateDto, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Distance, Is.EqualTo(560000));
        Assert.That(result.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(5.5)));

        routeServiceMock.Verify(
            r => r.GetRouteAsync("Vienna", "Munich", TransportType.Vacation),
            Times.Once);
    }

    [Test]
    public async Task GetAllToursAsync_MapsEntitiesToDtos_AndReturnsAll()
    {
        var toursFromDb = new[]
        {
            new Tour { Id = 1, UserId = 1, Name = "Alpine Hike", Distance = 25000, From = "Vienna", To = "Graz" },
            new Tour { Id = 2, UserId = 1, Name = "City Tour", Distance = 50000, From = "Vienna", To = "Prague" }
        };

        tourRepositoryMock.Setup(r => r.GetAllAsync(1))
            .ReturnsAsync(toursFromDb);

        var result = await tourService.GetAllToursAsync(1);

        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList[0].Name, Is.EqualTo("Alpine Hike"));
        Assert.That(resultList[1].Distance, Is.EqualTo(50000));
        Assert.That(resultList[0].From, Is.EqualTo("Vienna"));
    }

    [Test]
    public async Task GetTourByIdAsync_ReturnsNull_WhenTourDoesNotExist()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(999, 1))
            .ReturnsAsync((Tour?)null);

        var result = await tourService.GetTourByIdAsync(999, 1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetTourByIdAsync_ReturnsDtoWithCorrectData_WhenTourExists()
    {
        var existingTour = new Tour
        {
            Id = 42,
            UserId = 1,
            Name = "Mountain Tour",
            Distance = 150000,
            From = "Salzburg",
            To = "Innsbruck",
            RouteInformation = "Distance: 150.00 km"
        };

        tourRepositoryMock.Setup(r => r.GetByIdAsync(42, 1))
            .ReturnsAsync(existingTour);

        var result = await tourService.GetTourByIdAsync(42, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(42));
        Assert.That(result.Name, Is.EqualTo("Mountain Tour"));
        Assert.That(result.Distance, Is.EqualTo(150000));
    }

    [Test]
    public async Task DeleteTourAsync_ReturnsTrue_WhenTourIsDeleted()
    {
        tourRepositoryMock.Setup(r => r.DeleteAsync(7, 1))
            .ReturnsAsync(true);

        var result = await tourService.DeleteTourAsync(7, 1);

        Assert.That(result, Is.True);

        tourRepositoryMock.Verify(r => r.DeleteAsync(7, 1), Times.Once);
    }

    [Test]
    public async Task DeleteTourAsync_ReturnsFalse_WhenTourNotFoundForUser()
    {
        tourRepositoryMock.Setup(r => r.DeleteAsync(99, 1))
            .ReturnsAsync(false);

        var result = await tourService.DeleteTourAsync(99, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateTourAsync_ReturnsNull_WhenTourNotFoundForUser()
    {
        tourRepositoryMock.Setup(r => r.GetByIdAsync(5, 99))
            .ReturnsAsync((Tour?)null);

        var updateDto = new UpdateTourDto
        {
            Name = "Hacked Tour",
            From = "Vienna",
            To = "Prague",
            TransportType = TransportType.Vacation
        };

        var result = await tourService.UpdateTourAsync(5, updateDto, 99);

        Assert.That(result, Is.Null);

        routeServiceMock.Verify(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()), Times.Never);
    }

    [Test]
    public void UpdateTourAsync_ThrowsArgumentException_WhenFromIsEmpty()
    {
        var invalidDto = new UpdateTourDto { Name = "Tour", From = "", To = "Prague", TransportType = TransportType.Vacation };

        Assert.ThrowsAsync<ArgumentException>(() => tourService.UpdateTourAsync(5, invalidDto, 1));

        routeServiceMock.Verify(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()), Times.Never);
    }

    [Test]
    public async Task CreateTourAsync_HandlesDifferentTransportTypes()
    {
        var bikeDto = new CreateTourDto { Name = "Bike Tour", From = "Vienna", To = "Prague", TransportType = TransportType.Bike };
        var fakeRoute = new RouteResult { DistanceMeters = 330000, Duration = TimeSpan.FromHours(8) };

        routeServiceMock.Setup(r => r.GetRouteAsync("Vienna", "Prague", TransportType.Bike))
            .ReturnsAsync(fakeRoute);

        var savedTour = new Tour { Id = 1, UserId = 1, Distance = 330000 };
        tourRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
            .ReturnsAsync(savedTour);

        await tourService.CreateTourAsync(bikeDto, 1);

        routeServiceMock.Verify(r => r.GetRouteAsync("Vienna", "Prague", TransportType.Bike), Times.Once);
    }

    [Test]
    public async Task GetAllToursAsync_CallsRepositoryWithCorrectUserId()
    {
        tourRepositoryMock.Setup(r => r.GetAllAsync(5))
            .ReturnsAsync([]);

        await tourService.GetAllToursAsync(5);

        tourRepositoryMock.Verify(r => r.GetAllAsync(5), Times.Once);
        tourRepositoryMock.Verify(r => r.GetAllAsync(It.IsNotIn(5)), Times.Never);
    }

    [Test]
    public async Task CreateTourAsync_SetCreatedAtTimestamp()
    {
        var createDto = new CreateTourDto { Name = "Timestamped Tour", From = "Vienna", To = "Prague", TransportType = TransportType.Vacation };
        var fakeRoute = new RouteResult { DistanceMeters = 300000, Duration = TimeSpan.FromHours(3) };

        routeServiceMock.Setup(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()))
            .ReturnsAsync(fakeRoute);

        var beforeCreate = DateTime.UtcNow;
        Tour? capturedTour = null;

        tourRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
            .Callback<Tour>(t => capturedTour = t)
            .ReturnsAsync(new Tour { Id = 1, UserId = 1 });

        await tourService.CreateTourAsync(createDto, 1);

        Assert.That(capturedTour, Is.Not.Null);
        Assert.That(capturedTour!.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreate));
        Assert.That(capturedTour.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task GetAllToursAsync_UserOneSeesOnlyOwnTours_NotAnotherUsersTours()
    {
        var user1Tours = new[] { new Tour { Id = 1, UserId = 1, Name = "User 1 Tour A" } };
        var user2Tours = new[] { new Tour { Id = 2, UserId = 2, Name = "User 2 Tour" } };

        tourRepositoryMock.Setup(r => r.GetAllAsync(1))
            .ReturnsAsync(user1Tours);

        tourRepositoryMock.Setup(r => r.GetAllAsync(2))
            .ReturnsAsync(user2Tours);

        var result1 = await tourService.GetAllToursAsync(1);
        var result2 = await tourService.GetAllToursAsync(2);

        var list1 = result1.ToList();
        var list2 = result2.ToList();

        Assert.That(list1.Count, Is.EqualTo(1));
        Assert.That(list1[0].Name, Is.EqualTo("User 1 Tour A"));
        Assert.That(list2.Count, Is.EqualTo(1));
        Assert.That(list2[0].Name, Is.EqualTo("User 2 Tour"));

        tourRepositoryMock.Verify(r => r.GetAllAsync(1), Times.Once);
        tourRepositoryMock.Verify(r => r.GetAllAsync(2), Times.Once);
    }

    [Test]
    public async Task UpdateTourAsync_PreservesUserIdAndCantChangeOwner()
    {
        var existingTour = new Tour { Id = 5, UserId = 1, Name = "Original", From = "Vienna", To = "Graz", Distance = 100000 };
        tourRepositoryMock.Setup(r => r.GetByIdAsync(5, 1)).ReturnsAsync(existingTour);

        var fakeRoute = new RouteResult { DistanceMeters = 200000, Duration = TimeSpan.FromHours(2) };
        routeServiceMock.Setup(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()))
            .ReturnsAsync(fakeRoute);

        Tour? capturedTour = null;
        tourRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tour>(), 1))
            .Callback<Tour, int>((t, u) => capturedTour = t)
            .ReturnsAsync(new Tour { Id = 5, UserId = 1, Name = "Updated", Distance = 200000 });

        var updateDto = new UpdateTourDto { Name = "Updated", From = "Vienna", To = "Prague", TransportType = TransportType.Vacation };
        await tourService.UpdateTourAsync(5, updateDto, 1);

        Assert.That(capturedTour, Is.Not.Null);
        Assert.That(capturedTour!.UserId, Is.EqualTo(1));
    }
}
