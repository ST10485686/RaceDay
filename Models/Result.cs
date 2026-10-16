// Models/Result.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Result
{
    [Key]
    public int ResultID { get; set; }

    [Required]
    public int EnrolmentID { get; set; }

    public int? Position { get; set; }

    public TimeSpan? FinalTime { get; set; }

    public int? Points { get; set; }

    [MaxLength(255)]
    public string? Penalties { get; set; }

    public bool IsVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("EnrolmentID")]
    public EventEnrolment Enrolment { get; set; } = null!;
}