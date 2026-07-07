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

    // Test 1: CreateTourAsync ruft ORS auf und speichert mit echten Route-Daten
    [Test]
    public async Task CreateTourAsync_CallsRouteService_AndSavesToRepository()
    {
        // Arrange
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

        // Act
        var result = await tourService.CreateTourAsync(createDto, userId: 1);

        // Assert
        Assert.That(result.Distance, Is.EqualTo(330000));
        Assert.That(result.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(3.5)));
        Assert.That(result.RouteInformation, Is.EqualTo("Distance: 330.00 km, Duration: 03:30:00"));

        // Verify: RouteService wurde mit richtigen Parametern aufgerufen
        routeServiceMock.Verify(
            r => r.GetRouteAsync("Vienna", "Prague", TransportType.Vacation),
            Times.Once);

        // Verify: Repository wurde mit Tour aufgerufen die Distance von ORS enthält
        tourRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Tour>(t =>
                t.Distance == 330000 &&
                t.UserId == 1 &&
                t.Name == "Vienna to Prague")),
            Times.Once);
    }

    // Test 2: UpdateTourAsync recalculated Route wenn From/To sich ändern
    [Test]
    public async Task UpdateTourAsync_RecalculatesRoute_WhenFromToChanges()
    {
        // Arrange
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

        // Act
        var result = await tourService.UpdateTourAsync(5, updateDto, 1);

        // Assert - neue Route wurde berechnet
        Assert.That(result.Distance, Is.EqualTo(560000)); // nicht mehr 200000
        Assert.That(result.EstimatedTime, Is.EqualTo(TimeSpan.FromHours(5.5)));

        // Verify: ORS wurde aufgerufen mit NEUEN Koordinaten
        routeServiceMock.Verify(
            r => r.GetRouteAsync("Vienna", "Munich", TransportType.Vacation),
            Times.Once);
    }

    // Test 3: GetAllToursAsync mappt Entities zu DTOs
    [Test]
    public async Task GetAllToursAsync_MapsEntitiesToDtos_AndReturnsAll()
    {
        // Arrange
        var toursFromDb = new[]
        {
            new Tour { Id = 1, UserId = 1, Name = "Alpine Hike", Distance = 25000, From = "Vienna", To = "Graz" },
            new Tour { Id = 2, UserId = 1, Name = "City Tour", Distance = 50000, From = "Vienna", To = "Prague" }
        };

        tourRepositoryMock.Setup(r => r.GetAllAsync(1))
            .ReturnsAsync(toursFromDb);

        // Act
        var result = await tourService.GetAllToursAsync(1);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList[0].Name, Is.EqualTo("Alpine Hike"));
        Assert.That(resultList[1].Distance, Is.EqualTo(50000));
        Assert.That(resultList[0].From, Is.EqualTo("Vienna"));
    }

    // Test 4: GetTourByIdAsync gibt null zurück wenn Tour nicht existiert
    [Test]
    public async Task GetTourByIdAsync_ReturnsNull_WhenTourDoesNotExist()
    {
        // Arrange
        tourRepositoryMock.Setup(r => r.GetByIdAsync(999, 1))
            .ReturnsAsync((Tour?)null);

        // Act
        var result = await tourService.GetTourByIdAsync(999, 1);

        // Assert
        Assert.That(result, Is.Null);
    }

    // Test 5: GetTourByIdAsync gibt DTO zurück wenn Tour existiert
    [Test]
    public async Task GetTourByIdAsync_ReturnsDtoWithCorrectData_WhenTourExists()
    {
        // Arrange
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

        // Act
        var result = await tourService.GetTourByIdAsync(42, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(42));
        Assert.That(result.Name, Is.EqualTo("Mountain Tour"));
        Assert.That(result.Distance, Is.EqualTo(150000));
    }

    // Test 6: DeleteTourAsync gibt true zurück wenn erfolgreich
    [Test]
    public async Task DeleteTourAsync_ReturnsTrue_WhenTourIsDeleted()
    {
        // Arrange
        tourRepositoryMock.Setup(r => r.DeleteAsync(7, 1))
            .ReturnsAsync(true);

        // Act
        var result = await tourService.DeleteTourAsync(7, 1);

        // Assert
        Assert.That(result, Is.True);

        // Verify: Repository wurde aufgerufen mit Tour ID und User ID
        tourRepositoryMock.Verify(r => r.DeleteAsync(7, 1), Times.Once);
    }

    // Test 7: DeleteTourAsync gibt false zurück wenn Tour nicht gehört zu User
    [Test]
    public async Task DeleteTourAsync_ReturnsFalse_WhenTourNotFoundForUser()
    {
        // Arrange
        tourRepositoryMock.Setup(r => r.DeleteAsync(99, 1))
            .ReturnsAsync(false); // Repository sagt: nicht gefunden/nicht dein

        // Act
        var result = await tourService.DeleteTourAsync(99, 1);

        // Assert
        Assert.That(result, Is.False); // Sicherheit: User sieht nicht "nicht gefunden" vs "nicht dein"
    }


    // Test 10: UpdateTourAsync gibt null zurück wenn Tour nicht gehört zu User (Ownership)
    [Test]
    public async Task UpdateTourAsync_ReturnsNull_WhenTourNotFoundForUser()
    {
        // Arrange - User 99 versucht Tour von User 1 zu editieren
        tourRepositoryMock.Setup(r => r.GetByIdAsync(5, 99))
            .ReturnsAsync((Tour?)null); // Tour existiert nicht für diesen User

        var updateDto = new UpdateTourDto
        {
            Name = "Hacked Tour",
            From = "Vienna",
            To = "Prague",
            TransportType = TransportType.Vacation
        };

        // Act
        var result = await tourService.UpdateTourAsync(5, updateDto, 99);

        // Assert
        Assert.That(result, Is.Null); // Sicherheit: gibt null statt Exception/Error-Details

        // Verify: RouteService wurde NICHT aufgerufen (nicht autorisiert)
        routeServiceMock.Verify(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()), Times.Never);
    }

    // Test 11: UpdateTourAsync throwt Exception bei ungültigen Input-Daten
    [Test]
    public void UpdateTourAsync_ThrowsArgumentException_WhenFromIsEmpty()
    {
        // Arrange
        var invalidDto = new UpdateTourDto { Name = "Tour", From = "", To = "Prague", TransportType = TransportType.Vacation };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => tourService.UpdateTourAsync(5, invalidDto, 1));

        // Verify: RouteService wurde nicht aufgerufen (Validation war vorher)
        routeServiceMock.Verify(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()), Times.Never);
    }

    // Test 12: CreateTourAsync mit verschiedenen TransportTypes mappt zu richtigen Profilen
    [Test]
    public async Task CreateTourAsync_HandlesDifferentTransportTypes()
    {
        // Arrange - Test dass Service TransportType richtig an ORS weitergibt
        var bikeDto = new CreateTourDto { Name = "Bike Tour", From = "Vienna", To = "Prague", TransportType = TransportType.Bike };
        var fakeRoute = new RouteResult { DistanceMeters = 330000, Duration = TimeSpan.FromHours(8) };

        routeServiceMock.Setup(r => r.GetRouteAsync("Vienna", "Prague", TransportType.Bike))
            .ReturnsAsync(fakeRoute);

        var savedTour = new Tour { Id = 1, UserId = 1, Distance = 330000 };
        tourRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
            .ReturnsAsync(savedTour);

        // Act
        await tourService.CreateTourAsync(bikeDto, 1);

        // Assert & Verify
        routeServiceMock.Verify(r => r.GetRouteAsync("Vienna", "Prague", TransportType.Bike), Times.Once);
    }

    // Test 13: GetAllToursAsync respektiert User-Scoping (nur Touren des Users)
    [Test]
    public async Task GetAllToursAsync_CallsRepositoryWithCorrectUserId()
    {
        // Arrange - Repository wird mit User-ID aufgerufen
        tourRepositoryMock.Setup(r => r.GetAllAsync(5))
            .ReturnsAsync([]);

        // Act
        await tourService.GetAllToursAsync(5);

        // Assert & Verify - Repository wurde mit User ID 5 aufgerufen
        tourRepositoryMock.Verify(r => r.GetAllAsync(5), Times.Once);
        tourRepositoryMock.Verify(r => r.GetAllAsync(It.IsNotIn(5)), Times.Never);
    }

    // Test 14: CreateTourAsync inkludiert korrekte Timestamps
    [Test]
    public async Task CreateTourAsync_SetCreatedAtTimestamp()
    {
        // Arrange
        var createDto = new CreateTourDto { Name = "Timestamped Tour", From = "Vienna", To = "Prague", TransportType = TransportType.Vacation };
        var fakeRoute = new RouteResult { DistanceMeters = 300000, Duration = TimeSpan.FromHours(3) };

        routeServiceMock.Setup(r => r.GetRouteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransportType>()))
            .ReturnsAsync(fakeRoute);

        var beforeCreate = DateTime.UtcNow;
        Tour? capturedTour = null;

        tourRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tour>()))
            .Callback<Tour>(t => capturedTour = t)
            .ReturnsAsync(new Tour { Id = 1, UserId = 1 });

        // Act
        await tourService.CreateTourAsync(createDto, 1);

        // Assert - CreatedAt sollte zwischen beforeCreate und jetzt sein
        Assert.That(capturedTour, Is.Not.Null);
        Assert.That(capturedTour!.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreate));
        Assert.That(capturedTour.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }
}
