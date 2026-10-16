// Models/Document.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Document
{
    [Key]
    public int DocumentID { get; set; }

    [Required]
    public int EventID { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FileType { get; set; }

    [Required]
    public int UploadedBy { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }

    [ForeignKey("EventID")]
    public Event Event { get; set; } = null!;
}