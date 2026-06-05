using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Repositories.Interfaces;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepository;
    private readonly IJwtTokenService jwtTokenService;
    private readonly PasswordHasher<User> passwordHasher = new();

    public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        this.userRepository = userRepository;
        this.jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
    {
        await ValidateRegistrationAsync(dto);

        User user = new()
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        User createdUser = await userRepository.CreateAsync(user);

        return MapToAuthResponseDto(createdUser);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return null;
        }

        User? user = await userRepository.GetByEmailAsync(dto.Email.Trim());
        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return MapToAuthResponseDto(user);
    }

    private async Task ValidateRegistrationAsync(RegisterUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email must not be empty.");
        }

        if (!IsValidEmail(dto.Email))
        {
            throw new ArgumentException("Email must have a valid format.");
        }

        if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters.");
        }

        if (await userRepository.UsernameExistsAsync(dto.Username.Trim()))
        {
            throw new ArgumentException("Username is already taken.");
        }

        if (await userRepository.EmailExistsAsync(dto.Email.Trim()))
        {
            throw new ArgumentException("Email address is already registered.");
        }
    }

    private AuthResponseDto MapToAuthResponseDto(User user)
    {
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = jwtTokenService.GenerateToken(user)
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
