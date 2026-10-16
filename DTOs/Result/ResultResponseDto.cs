public class ResultResponseDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public int? Position { get; set; }
    public TimeSpan? FinalTime { get; set; }
    public int? Points { get; set; }
    public string? Penalties { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ParticipantName { get; set; }
    public string? CategoryName { get; set; }
    public string? EventTitle { get; set; }
}