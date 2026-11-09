using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models.ViewModels;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Participant"; // Organiser or Participant

    // Organiser fields
    public string? OrganisationName { get; set; }
    public string? ContactPhone { get; set; }

    // Participant fields
    public string? SailNumber { get; set; }
    public string? BoatName { get; set; }
}