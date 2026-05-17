using Microsoft.AspNetCore.Mvc;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            message = "TourPlanner API is running"
        });
    }
}
