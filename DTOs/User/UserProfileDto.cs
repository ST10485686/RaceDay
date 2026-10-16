// DTOs/User/UserProfileDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.User;

public class UserProfileDto
{
    public int UserID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }

    // Organiser fields
    public string? OrganisationName { get; set; }
    public string? ContactPhone { get; set; }

    // Participant fields
    public string? SailNumber { get; set; }
    public string? BoatName { get; set; }
    public string? BoatType { get; set; }
    public string? ClubName { get; set; }
}

public class UpdateUserProfileDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    // Participant-only fields
    public string? SailNumber { get; set; }
    public string? BoatName { get; set; }
    public string? BoatType { get; set; }
    public string? ClubName { get; set; }

    // Organiser-only fields
    public string? OrganisationName { get; set; }
    public string? ContactPhone { get; set; }
}