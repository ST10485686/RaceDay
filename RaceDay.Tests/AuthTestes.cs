using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Helpers;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests
{
    // ============================================================
    // HELPERS
    // ============================================================

    private RaceDayDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    private IConfiguration GetConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestKeyThatIsAtLeast32CharactersLongForTesting!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
        return config;
    }

    // ============================================================
    // REGISTRATION TESTS
    // ============================================================

    [Fact]
    public async Task Register_WithNewEmail_ReturnsToken()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        var dto = new RegisterDto
        {
            Email = "test@raceday.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            SailNumber = "SA-0001"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal("test@raceday.com", result.User.Email);
        Assert.Equal("Participant", result.User.Role);
    }

    [Fact]
    public async Task Register_Organiser_CreatesOrganiserProfile()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        var dto = new RegisterDto
        {
            Email = "organiser@raceday.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "Organiser",
            Role = "Organiser",
            OrganisationName = "Test Sailing Club",
            ContactPhone = "+27 82 111 2222"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Organiser", result.User.Role);

        var organiser = await db.Organisers.FirstOrDefaultAsync(o => o.UserID == result.User.UserID);
        Assert.NotNull(organiser);
        Assert.Equal("Test Sailing Club", organiser!.OrganisationName);
    }

    [Fact]
    public async Task Register_Participant_CreatesParticipantProfile()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        var dto = new RegisterDto
        {
            Email = "participant@raceday.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "Participant",
            Role = "Participant",
            SailNumber = "SA-9999",
            BoatName = "Test Boat",
            BoatType = "Nacra 17",
            ClubName = "Test Club"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Participant", result.User.Role);

        var participant = await db.Participants.FirstOrDefaultAsync(p => p.UserID == result.User.UserID);
        Assert.NotNull(participant);
        Assert.Equal("SA-9999", participant!.SailNumber);
        Assert.Equal("Test Boat", participant.BoatName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsNull()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        var dto = new RegisterDto
        {
            Email = "dup@raceday.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            SailNumber = "SA-0002"
        };
        await service.RegisterAsync(dto);

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Register_WithInvalidRole_ThrowsException()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        var dto = new RegisterDto
        {
            Email = "bad@raceday.com",
            Password = "Password123!",
            FirstName = "Bad",
            LastName = "Role",
            Role = "Admin" // Invalid role
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(dto));
    }

    // ============================================================
    // LOGIN TESTS
    // ============================================================

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        await service.RegisterAsync(new RegisterDto
        {
            Email = "login@raceday.com",
            Password = "CorrectPassword!",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            SailNumber = "SA-0003"
        });

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "login@raceday.com",
            Password = "CorrectPassword!"
        });

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal("login@raceday.com", result.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        await service.RegisterAsync(new RegisterDto
        {
            Email = "login2@raceday.com",
            Password = "CorrectPassword!",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            SailNumber = "SA-0004"
        });

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "login2@raceday.com",
            Password = "WrongPassword!"
        });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsNull()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "ghost@raceday.com",
            Password = "Password123!"
        });

        // Assert
        Assert.Null(result);
    }

    // ============================================================
    // ROLE-BASED TOKEN TESTS
    // ============================================================

    [Fact]
    public async Task Login_AsOrganiser_TokenContainsOrganiserRole()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        await service.RegisterAsync(new RegisterDto
        {
            Email = "org@raceday.com",
            Password = "Password123!",
            FirstName = "Org",
            LastName = "User",
            Role = "Organiser",
            OrganisationName = "Test Org",
            ContactPhone = "+27 82 000 0000"
        });

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "org@raceday.com",
            Password = "Password123!"
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Organiser", result!.User.Role);
    }

    [Fact]
    public async Task Login_AsParticipant_TokenContainsParticipantRole()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new AuthService(db, GetConfig());
        await service.RegisterAsync(new RegisterDto
        {
            Email = "part@raceday.com",
            Password = "Password123!",
            FirstName = "Part",
            LastName = "User",
            Role = "Participant",
            SailNumber = "SA-1111"
        });

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "part@raceday.com",
            Password = "Password123!"
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Participant", result!.User.Role);
    }

    // ============================================================
    // PASSWORD HASHING TESTS
    // ============================================================

    [Fact]
    public void PasswordHasher_ProducesConsistentHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void PasswordHasher_VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "CorrectPassword!";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PasswordHasher_VerifyPassword_ReturnsFalseForWrongPassword()
    {
        // Arrange
        var hash = PasswordHasher.HashPassword("CorrectPassword!");

        // Act
        var result = PasswordHasher.VerifyPassword("WrongPassword!", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PasswordHasher_NeverReturnsPlainTextPassword()
    {
        // Arrange
        var password = "MySecretPassword!";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.DoesNotContain(password, hash);
    }
}
