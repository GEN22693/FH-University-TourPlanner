using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using TourPlanner.Business.Interfaces;
using TourPlanner.Data.Storage;
using TourPlanner.Models;
using TourPlanner.Models.Dtos;

namespace TourPlanner.Business.Services;

public class AuthService : IAuthService
{
    private readonly InMemoryDataStore dataStore;
    private readonly IJwtTokenService jwtTokenService;
    private readonly PasswordHasher<User> passwordHasher = new();

    public AuthService(InMemoryDataStore dataStore, IJwtTokenService jwtTokenService)
    {
        this.dataStore = dataStore;
        this.jwtTokenService = jwtTokenService;
    }

    public Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
    {
        ValidateRegistration(dto);

        User user = new()
        {
            Id = GetNextUserId(),
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        dataStore.Users.Add(user);

        return Task.FromResult(MapToAuthResponseDto(user));
    }

    public Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }

        User? user = dataStore.Users.FirstOrDefault(user =>
            string.Equals(user.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }

        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }

        return Task.FromResult<AuthResponseDto?>(MapToAuthResponseDto(user));
    }

    private void ValidateRegistration(RegisterUserDto dto)
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

        if (dataStore.Users.Any(user => string.Equals(user.Username, dto.Username.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Username is already taken.");
        }

        if (dataStore.Users.Any(user => string.Equals(user.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Email address is already registered.");
        }
    }

    private int GetNextUserId()
    {
        return dataStore.Users.Count == 0 ? 1 : dataStore.Users.Max(user => user.Id) + 1;
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
