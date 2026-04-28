using Microsoft.AspNetCore.Mvc;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return "TourPlanner API is running";
    }
}
