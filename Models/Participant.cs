// Models/Participant.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Participant
{
    [Key]
    public int ParticipantID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required, MaxLength(50)]
    public string SailNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? BoatName { get; set; }

    [MaxLength(100)]
    public string? BoatType { get; set; }

    [MaxLength(100)]
    public string? ClubName { get; set; }

    [ForeignKey("UserID")]
    public User User { get; set; } = null!;

    public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}