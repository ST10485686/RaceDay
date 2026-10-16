// Models/Event.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace RaceDay.API.Models;

public class Event
{
    [Key]
    public int EventID { get; set; }

    [Required]
    public int OrganiserID { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime DateStart { get; set; }

    [Required]
    public DateTime DateEnd { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Open, InProgress, Completed, Cancelled

    public int? MaxParticipants { get; set; }

    [Required, MaxLength(50)]
    public string EventType { get; set; } = "Race"; // Race, Regatta, Series

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("OrganiserID")]
    public Organiser Organiser { get; set; } = null!;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}