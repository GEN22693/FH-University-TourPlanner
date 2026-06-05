using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);

    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
