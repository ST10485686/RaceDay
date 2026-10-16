// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Helpers;
using RaceDay.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.API.Services;

public class AuthService : IAuthService
{
    private readonly RaceDayDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(RaceDayDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        // Validate role
        if (dto.Role != "Organiser" && dto.Role != "Participant")
            throw new ArgumentException("Role must be either 'Organiser' or 'Participant'");

        // Create user
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create role-specific profile
        if (dto.Role == "Organiser")
        {
            var organiser = new Organiser
            {
                UserID = user.UserID,
                OrganisationName = dto.OrganisationName ?? $"{dto.FirstName}'s Organisation",
                ContactPhone = dto.ContactPhone ?? "N/A",
                IsVerified = false
            };
            _context.Organisers.Add(organiser);
        }
        else
        {
            var participant = new Participant
            {
                UserID = user.UserID,
                SailNumber = dto.SailNumber ?? $"SA-{new Random().Next(1000, 9999)}",
                BoatName = dto.BoatName,
                BoatType = dto.BoatType,
                ClubName = dto.ClubName
            };
            _context.Participants.Add(participant);
        }

        await _context.SaveChangesAsync();

        // Generate token
        var token = GenerateJwtToken(user.UserID, user.Email, user.Role, $"{user.FirstName} {user.LastName}");

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth
            }
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return null;

        if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        var token = GenerateJwtToken(user.UserID, user.Email, user.Role, $"{user.FirstName} {user.LastName}");

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                UserID = user.UserID,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth
            }
        };
    }

    public string GenerateJwtToken(int userId, string email, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, fullName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "RaceDayAPI",
            audience: _config["Jwt:Audience"] ?? "RaceDayClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}