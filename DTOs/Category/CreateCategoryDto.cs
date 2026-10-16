// DTOs/Category/CreateCategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs.Category;

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? MinBoatCount { get; set; }
    public int? MaxBoatCount { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto { }