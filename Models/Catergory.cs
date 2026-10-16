// Models/Category.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    [Required]
    public int EventID { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;


    public string? Description { get; set; }

    public int? MinBoatCount { get; set; }
    public int? MaxBoatCount { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey("EventID")]
    public Event Event { get; set; } = null!;

    public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}