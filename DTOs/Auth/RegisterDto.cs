// DTOs/Auth/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Auth;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [Required]
    public string Role { get; set; } = "Participant"; // Organiser or Participant

    // Organiser fields (required if Role = Organiser)
    public string? OrganisationName { get; set; }
    public string? ContactPhone { get; set; }

    // Participant fields (required if Role = Participant)
    public string? SailNumber { get; set; }
    public string? BoatName { get; set; }
    public string? BoatType { get; set; }
    public string? ClubName { get; set; }
}