using TourPlanner.Models;

namespace TourPlanner.Business.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
