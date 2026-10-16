// DTOs/Result/CreateResultDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Result;

public class CreateResultDto
{
    public int? Position { get; set; }
    public TimeSpan? FinalTime { get; set; }
    public int? Points { get; set; }
    public string? Penalties { get; set; }
}

public class ResultResponseDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public int? Position { get; set; }
    public TimeSpan? FinalTime { get; set; }
    public int? Points { get; set; }
    public string? Penalties { get; set; }
    public bool IsVerified { get; set; }
}