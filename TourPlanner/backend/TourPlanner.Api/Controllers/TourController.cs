using Microsoft.AspNetCore.Mvc;
using TourPlanner.Business.Interfaces;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/tours")]
public class TourController : ControllerBase
{
    private readonly ITourService tourService;

    public TourController(ITourService tourService)
    {
        this.tourService = tourService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TourResponseDto>>> GetAllTours()
    {
        IEnumerable<TourResponseDto> tours = await tourService.GetAllToursAsync();
        return Ok(tours);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TourResponseDto>> GetTourById(int id)
    {
        TourResponseDto? tour = await tourService.GetTourByIdAsync(id);
        if (tour is null)
        {
            return NotFound();
        }

        return Ok(tour);
    }

    [HttpPost]
    public async Task<ActionResult<TourResponseDto>> CreateTour(CreateTourDto dto)
    {
        try
        {
            TourResponseDto createdTour = await tourService.CreateTourAsync(dto);
            return CreatedAtAction(nameof(GetTourById), new { id = createdTour.Id }, createdTour);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TourResponseDto>> UpdateTour(int id, UpdateTourDto dto)
    {
        try
        {
            TourResponseDto? updatedTour = await tourService.UpdateTourAsync(id, dto);
            if (updatedTour is null)
            {
                return NotFound();
            }

            return Ok(updatedTour);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTour(int id)
    {
        bool wasDeleted = await tourService.DeleteTourAsync(id);
        if (!wasDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
