using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.Business.Interfaces;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
    {
        try
        {
            AuthResponseDto response = await authService.RegisterAsync(dto);
            return Created("/api/auth/me", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        AuthResponseDto? response = await authService.LoginAsync(dto);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? username = User.FindFirstValue(ClaimTypes.Name);
        string? email = User.FindFirstValue(ClaimTypes.Email);

        if (userId is null || username is null || email is null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            userId = int.Parse(userId),
            username,
            email
        });
    }
}
