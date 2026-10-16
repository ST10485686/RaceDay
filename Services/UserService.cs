// Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.User;

namespace RaceDay.API.Services;

public class UserService : IUserService
{
    private readonly RaceDayDbContext _context;

    public UserService(RaceDayDbContext context) => _context = context;

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.OrganiserProfile)
            .Include(u => u.ParticipantProfile)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return null;

        return new UserProfileDto
        {
            UserID = user.UserID,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            DateOfBirth = user.DateOfBirth,
            CreatedAt = user.CreatedAt,
            OrganisationName = user.OrganiserProfile?.OrganisationName,
            ContactPhone = user.OrganiserProfile?.ContactPhone,
            SailNumber = user.ParticipantProfile?.SailNumber,
            BoatName = user.ParticipantProfile?.BoatName,
            BoatType = user.ParticipantProfile?.BoatType,
            ClubName = user.ParticipantProfile?.ClubName
        };
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateUserProfileDto dto)
    {
        var user = await _context.Users
            .Include(u => u.OrganiserProfile)
            .Include(u => u.ParticipantProfile)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return null;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DateOfBirth = dto.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Role == "Participant" && user.ParticipantProfile != null)
        {
            if (dto.SailNumber != null) user.ParticipantProfile.SailNumber = dto.SailNumber;
            if (dto.BoatName != null) user.ParticipantProfile.BoatName = dto.BoatName;
            if (dto.BoatType != null) user.ParticipantProfile.BoatType = dto.BoatType;
            if (dto.ClubName != null) user.ParticipantProfile.ClubName = dto.ClubName;
        }
        else if (user.Role == "Organiser" && user.OrganiserProfile != null)
        {
            if (dto.OrganisationName != null) user.OrganiserProfile.OrganisationName = dto.OrganisationName;
            if (dto.ContactPhone != null) user.OrganiserProfile.ContactPhone = dto.ContactPhone;
        }

        await _context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }
}