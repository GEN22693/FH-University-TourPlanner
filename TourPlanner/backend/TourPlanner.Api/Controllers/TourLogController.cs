using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Business.Interfaces;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/tours/{tourId:int}/logs")]
[Authorize]
public class TourLogController : ControllerBase
{
    private readonly ITourLogService tourLogService;

    public TourLogController(ITourLogService tourLogService)
    {
        this.tourLogService = tourLogService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TourLogResponseDto>>> GetLogsByTourId(int tourId)
    {
        try
        {
            IEnumerable<TourLogResponseDto> logs = await tourLogService.GetLogsByTourIdAsync(tourId, GetUserId());
            return Ok(logs);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("{logId:int}")]
    public async Task<ActionResult<TourLogResponseDto>> GetLogById(int tourId, int logId)
    {
        try
        {
            TourLogResponseDto? log = await tourLogService.GetLogByIdAsync(tourId, logId, GetUserId());
            if (log is null)
            {
                return NotFound();
            }

            return Ok(log);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<TourLogResponseDto>> CreateLog(int tourId, CreateTourLogDto dto)
    {
        try
        {
            TourLogResponseDto createdLog = await tourLogService.CreateLogAsync(tourId, dto, GetUserId());
            return CreatedAtAction(nameof(GetLogById), new { tourId, logId = createdLog.Id }, createdLog);
        }
        catch (ArgumentException ex) when (ex.Message == "Tour does not exist.")
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{logId:int}")]
    public async Task<ActionResult<TourLogResponseDto>> UpdateLog(int tourId, int logId, UpdateTourLogDto dto)
    {
        try
        {
            TourLogResponseDto? updatedLog = await tourLogService.UpdateLogAsync(tourId, logId, dto, GetUserId());
            if (updatedLog is null)
            {
                return NotFound();
            }

            return Ok(updatedLog);
        }
        catch (ArgumentException ex) when (ex.Message == "Tour does not exist.")
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{logId:int}")]
    public async Task<IActionResult> DeleteLog(int tourId, int logId)
    {
        try
        {
            bool wasDeleted = await tourLogService.DeleteLogAsync(tourId, logId, GetUserId());
            if (!wasDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
}
