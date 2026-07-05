using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Business.Interfaces;
using TourPlanner.Models.Dtos;
using TourPlanner.Models.Enums;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/tours")]
[Authorize]
public class TourController : ControllerBase
{
    private readonly ITourService tourService;
    private readonly ITourLogService tourLogService;

    public TourController(ITourService tourService, ITourLogService tourLogService)
    {
        this.tourService = tourService;
        this.tourLogService = tourLogService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TourResponseDto>>> GetAllTours()
    {
        List<TourResponseDto> tours = (await tourService.GetAllToursAsync(GetUserId())).ToList();

        foreach (TourResponseDto tour in tours)
        {
            await EnrichTourWithComputedValuesAsync(tour);
        }

        return Ok(tours);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TourResponseDto>>> SearchTours([FromQuery] string query)
    {
        List<TourResponseDto> tours = (await tourService.GetAllToursAsync(GetUserId())).ToList();
        List<TourResponseDto> result = [];

        string normalizedQuery = query.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            foreach (TourResponseDto tour in tours)
            {
                await EnrichTourWithComputedValuesAsync(tour);
            }

            return Ok(tours);
        }

        foreach (TourResponseDto tour in tours)
        {
            List<TourLogResponseDto> logs =
                (await tourLogService.GetLogsByTourIdAsync(tour.Id, GetUserId())).ToList();

            ApplyComputedValues(tour, logs);

            string searchText = BuildSearchText(tour, logs);

            if (searchText.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(tour);
            }
        }

        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<TourStatisticsDto>> GetStatistics()
    {
        List<TourResponseDto> tours = (await tourService.GetAllToursAsync(GetUserId())).ToList();

        int totalLogs = 0;
        double ratingSum = 0;
        int ratingCount = 0;

        string mostPopularTourName = string.Empty;
        int highestPopularity = -1;

        string bestRatedTourName = string.Empty;
        double highestAverageRating = -1;

        foreach (TourResponseDto tour in tours)
        {
            List<TourLogResponseDto> logs =
                (await tourLogService.GetLogsByTourIdAsync(tour.Id, GetUserId())).ToList();

            ApplyComputedValues(tour, logs);

            totalLogs += logs.Count;

            foreach (TourLogResponseDto log in logs)
            {
                ratingSum += log.Rating;
                ratingCount++;
            }

            if (tour.Popularity > highestPopularity)
            {
                highestPopularity = tour.Popularity;
                mostPopularTourName = tour.Name;
            }

            if (logs.Count > 0)
            {
                double averageTourRating = logs.Average(log => log.Rating);

                if (averageTourRating > highestAverageRating)
                {
                    highestAverageRating = averageTourRating;
                    bestRatedTourName = tour.Name;
                }
            }
        }

        TourStatisticsDto statistics = new()
        {
            TotalTours = tours.Count,
            TotalLogs = totalLogs,
            TotalDistance = tours.Sum(tour => tour.Distance),
            TotalEstimatedTime = TimeSpan.FromTicks(tours.Sum(tour => tour.EstimatedTime.Ticks)),
            AverageRating = ratingCount == 0 ? 0 : Math.Round(ratingSum / ratingCount, 1),
            MostPopularTourName = mostPopularTourName,
            BestRatedTourName = bestRatedTourName,
        };

        return Ok(statistics);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportTours()
    {
        List<TourResponseDto> tours = (await tourService.GetAllToursAsync(GetUserId())).ToList();
        List<TourImportExportDto> exportTours = [];

        foreach (TourResponseDto tour in tours)
        {
            List<TourLogResponseDto> logs =
                (await tourLogService.GetLogsByTourIdAsync(tour.Id, GetUserId())).ToList();

            ApplyComputedValues(tour, logs);

            exportTours.Add(new TourImportExportDto
            {
                Name = tour.Name,
                Description = tour.Description,
                From = tour.From,
                To = tour.To,
                TransportType = tour.TransportType,
                Distance = tour.Distance,
                EstimatedTime = tour.EstimatedTime,
                RouteInformation = tour.RouteInformation,
                Popularity = tour.Popularity,
                ChildFriendliness = tour.ChildFriendliness,
                Logs = logs.Select(log => new TourLogImportExportDto
                {
                    DateTime = log.DateTime,
                    Comment = log.Comment,
                    Difficulty = log.Difficulty,
                    TotalDistance = log.TotalDistance,
                    TotalTime = log.TotalTime,
                    Rating = log.Rating,
                }).ToList(),
            });
        }

        string json = JsonSerializer.Serialize(exportTours, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        byte[] fileBytes = Encoding.UTF8.GetBytes(json);

        return File(fileBytes, "application/json", "tourplanner-export.json");
    }

    [HttpPost("import")]
    public async Task<ActionResult<IEnumerable<TourResponseDto>>> ImportTours(
        [FromBody] List<TourImportExportDto> importedTours)
    {
        if (importedTours.Count == 0)
        {
            return BadRequest(new { message = "Import file contains no tours." });
        }

        List<TourResponseDto> createdTours = [];

        try
        {
            foreach (TourImportExportDto importedTour in importedTours)
            {
                CreateTourDto createTourDto = new()
                {
                    Name = importedTour.Name,
                    Description = importedTour.Description,
                    From = importedTour.From,
                    To = importedTour.To,
                    TransportType = importedTour.TransportType,
                };

                TourResponseDto createdTour =
                    await tourService.CreateTourAsync(createTourDto, GetUserId());

                foreach (TourLogImportExportDto importedLog in importedTour.Logs)
                {
                    CreateTourLogDto createLogDto = new()
                    {
                        DateTime = importedLog.DateTime,
                        Comment = importedLog.Comment,
                        Difficulty = importedLog.Difficulty,
                        TotalDistance = importedLog.TotalDistance,
                        TotalTime = importedLog.TotalTime,
                        Rating = importedLog.Rating,
                    };

                    await tourLogService.CreateLogAsync(createdTour.Id, createLogDto, GetUserId());
                }

                TourResponseDto? refreshedTour =
                    await tourService.GetTourByIdAsync(createdTour.Id, GetUserId());

                if (refreshedTour is not null)
                {
                    await EnrichTourWithComputedValuesAsync(refreshedTour);
                    createdTours.Add(refreshedTour);
                }
            }

            return Ok(createdTours);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TourResponseDto>> GetTourById(int id)
    {
        TourResponseDto? tour = await tourService.GetTourByIdAsync(id, GetUserId());

        if (tour is null)
        {
            return NotFound();
        }

        await EnrichTourWithComputedValuesAsync(tour);

        return Ok(tour);
    }

    [HttpPost]
    public async Task<ActionResult<TourResponseDto>> CreateTour(CreateTourDto dto)
    {
        try
        {
            TourResponseDto createdTour = await tourService.CreateTourAsync(dto, GetUserId());
            await EnrichTourWithComputedValuesAsync(createdTour);

            return CreatedAtAction(nameof(GetTourById), new { id = createdTour.Id }, createdTour);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TourResponseDto>> UpdateTour(int id, UpdateTourDto dto)
    {
        try
        {
            TourResponseDto? updatedTour = await tourService.UpdateTourAsync(id, dto, GetUserId());

            if (updatedTour is null)
            {
                return NotFound();
            }

            await EnrichTourWithComputedValuesAsync(updatedTour);

            return Ok(updatedTour);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTour(int id)
    {
        bool wasDeleted = await tourService.DeleteTourAsync(id, GetUserId());

        if (!wasDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private async Task EnrichTourWithComputedValuesAsync(TourResponseDto tour)
    {
        List<TourLogResponseDto> logs =
            (await tourLogService.GetLogsByTourIdAsync(tour.Id, GetUserId())).ToList();

        ApplyComputedValues(tour, logs);
    }

    private static void ApplyComputedValues(TourResponseDto tour, List<TourLogResponseDto> logs)
    {
        tour.Popularity = logs.Count;
        tour.ChildFriendliness = CalculateChildFriendliness(logs);
    }

    private static string CalculateChildFriendliness(List<TourLogResponseDto> logs)
    {
        if (logs.Count == 0)
        {
            return "Unknown";
        }

        double averageDifficulty = logs.Average(log => DifficultyToNumber(log.Difficulty));
        double averageDistance = logs.Average(log => log.TotalDistance);
        double averageTimeInMinutes = logs.Average(log => log.TotalTime.TotalMinutes);

        if (averageDifficulty <= 1.5 && averageDistance <= 10 && averageTimeInMinutes <= 120)
        {
            return "High";
        }

        if (averageDifficulty <= 2.3 && averageDistance <= 20 && averageTimeInMinutes <= 240)
        {
            return "Medium";
        }

        return "Low";
    }

    private static int DifficultyToNumber(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => 1,
            Difficulty.Medium => 2,
            Difficulty.Hard => 3,
            _ => 3,
        };
    }

    private static string BuildSearchText(TourResponseDto tour, List<TourLogResponseDto> logs)
    {
        StringBuilder builder = new();

        builder.AppendLine(tour.Name);
        builder.AppendLine(tour.Description);
        builder.AppendLine(tour.From);
        builder.AppendLine(tour.To);
        builder.AppendLine(tour.TransportType.ToString());
        builder.AppendLine(tour.Distance.ToString());
        builder.AppendLine(tour.EstimatedTime.ToString());
        builder.AppendLine(tour.RouteInformation);
        builder.AppendLine(tour.Popularity.ToString());
        builder.AppendLine(tour.ChildFriendliness);

        foreach (TourLogResponseDto log in logs)
        {
            builder.AppendLine(log.DateTime.ToString("O"));
            builder.AppendLine(log.Comment);
            builder.AppendLine(log.Difficulty.ToString());
            builder.AppendLine(log.TotalDistance.ToString());
            builder.AppendLine(log.TotalTime.ToString());
            builder.AppendLine(log.Rating.ToString());
        }

        return builder.ToString().ToLowerInvariant();
    }
}