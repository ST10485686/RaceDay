// Models/EventEnrolment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class EventEnrolment
{
    [Key]
    public int EnrolmentID { get; set; }

    [Required]
    public int ParticipantID { get; set; }

    [Required]
    public int CategoryID { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Withdrawn, Waitlisted

    [Required, MaxLength(20)]
    public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded

    public string? Notes { get; set; }

    [ForeignKey("ParticipantID")]
    public Participant Participant { get; set; } = null!;

    [ForeignKey("CategoryID")]
    public Category Category { get; set; } = null!;

    public Result? Result { get; set; }
}