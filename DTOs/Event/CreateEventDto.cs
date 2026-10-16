// DTOs/Event/CreateEventDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Event;

public class CreateEventDto
{
    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime DateStart { get; set; }

    [Required]
    public DateTime DateEnd { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public int? MaxParticipants { get; set; }

    [Required, MaxLength(50)]
    public string EventType { get; set; } = "Race";
}

public class UpdateEventDto : CreateEventDto { }

public class EventResponseDto
{
    public int EventID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? MaxParticipants { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public List<CategoryResponseDto> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CategoryResponseDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MinBoatCount { get; set; }
    public int? MaxBoatCount { get; set; }
    public bool IsActive { get; set; }
}