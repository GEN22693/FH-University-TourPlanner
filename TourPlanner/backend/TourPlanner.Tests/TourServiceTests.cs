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
    //Validierung 
    [Test]
    public void CreateTourAsync_ThrowsArgumentException_WhenNameIsEmpty()
    {
        TourService service = new(null!, null!);
        Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTourAsync(new CreateTourDto { Name = "", From = "Vienna", To = "Salzburg" }, 1));
    }

    [Test]
    public void CreateTourAsync_ThrowsArgumentException_WhenFromIsEmpty()
    {
        TourService service = new(null!, null!);
        Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTourAsync(new CreateTourDto { Name = "Tour", From = "", To = "Salzburg" }, 1));
    }

    [Test]
    public void CreateTourAsync_ThrowsArgumentException_WhenToIsEmpty()
    {
        TourService service = new(null!, null!);
        Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTourAsync(new CreateTourDto { Name = "Tour", From = "Vienna", To = "" }, 1));
    }

    [Test]
    public void CreateTourAsync_ThrowsArgumentException_WhenFromAndToAreIdentical()
    {
        TourService service = new(null!, null!);
        Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTourAsync(new CreateTourDto { Name = "Tour", From = "Vienna", To = "Vienna" }, 1));
    }

    [Test]
    public void CreateTourAsync_ThrowsArgumentException_WhenFromAndToAreIdenticalCaseInsensitive()
    {
        TourService service = new(null!, null!);
        Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTourAsync(new CreateTourDto { Name = "Tour", From = "vienna", To = "VIENNA" }, 1));
    }

    //Happy Path

    [Test]
    public async Task GetAllToursAsync_ReturnsMappedDtos()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetAllAsync(1)).ReturnsAsync([
            new Tour { Id = 1, UserId = 1, Name = "Alpine Tour", From = "Vienna", To = "Salzburg" }
        ]);

        TourService service = new(repoMock.Object, null!);
        IEnumerable<TourResponseDto> result = await service.GetAllToursAsync(1);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Alpine Tour"));
    }

    [Test]
    public async Task GetTourByIdAsync_ReturnsNull_WhenTourDoesNotExist()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.GetByIdAsync(99, 1)).ReturnsAsync((Tour?)null);

        TourService service = new(repoMock.Object, null!);
        TourResponseDto? result = await service.GetTourByIdAsync(99, 1);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteTourAsync_ReturnsFalse_WhenTourDoesNotExist()
    {
        Mock<ITourRepository> repoMock = new();
        repoMock.Setup(r => r.DeleteAsync(99, 1)).ReturnsAsync(false);

        TourService service = new(repoMock.Object, null!);
        bool result = await service.DeleteTourAsync(99, 1);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CreateTourAsync_CreatesTourWithRouteData()
    {
        Mock<ITourRepository> repoMock = new();
        Mock<IRouteService> routeMock = new();

        RouteResult fakeRoute = new()
        {
            DistanceMeters = 298000,
            Duration = TimeSpan.FromHours(3),
            RouteInformation = "Distance: 298.00 km, Duration: 03:00:00"
        };
        routeMock.Setup(r => r.GetRouteAsync("Vienna", "Salzburg", TransportType.Vacation)).ReturnsAsync(fakeRoute);
        repoMock.Setup(r => r.CreateAsync(It.IsAny<Tour>())).ReturnsAsync(new Tour
        {
            Id = 1, Name = "City Tour", From = "Vienna", To = "Salzburg",
            Distance = 298000, EstimatedTime = TimeSpan.FromHours(3)
        });

        TourService service = new(repoMock.Object, routeMock.Object);
        TourResponseDto result = await service.CreateTourAsync(
            new CreateTourDto { Name = "City Tour", From = "Vienna", To = "Salzburg", TransportType = TransportType.Vacation }, 1);

        Assert.That(result.Distance, Is.EqualTo(298000));
        Assert.That(result.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(3)));
    }
}
