// DTOs/Enrolment/CreateEnrolmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Enrolment;

public class CreateEnrolmentDto
{
    [Required]
    public int EventID { get; set; }

    [Required]
    public int CategoryID { get; set; }

    public string? Notes { get; set; }
}

public class EnrolmentResponseDto
{
    public int EnrolmentID { get; set; }
    public int EventID { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateEnrolmentDto
{
    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(20)]
    public string? PaymentStatus { get; set; }

    public string? Notes { get; set; }
}