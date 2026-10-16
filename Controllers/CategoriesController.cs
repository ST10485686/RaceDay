using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Category;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all categories for a specific event.
    /// </summary>
    /// <param name="eventId">Event ID.</param>
    /// <response code="200">Returns the list of categories.</response>
    /// <response code="404">Event not found.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.EventID == eventId);
        if (!exists)
            return NotFound(new { message = $"Event with ID {eventId} not found." });

        var categories = await _context.Categories
            .Where(c => c.EventID == eventId)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get a single category by ID (within an event).
    /// </summary>
    [HttpGet("{categoryId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int eventId, int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.EventID == eventId);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        return Ok(MapToDto(category));
    }

    /// <summary>
    /// Create a new category for an event. Only the owning Organiser.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.MinBoatCount.HasValue && dto.MaxBoatCount.HasValue &&
            dto.MinBoatCount > dto.MaxBoatCount)
            return BadRequest(new { message = "MinBoatCount cannot exceed MaxBoatCount." });

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbidden();

        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (eventEntity == null)
            return NotFound(new { message = $"Event with ID {eventId} not found." });

        if (eventEntity.OrganiserID != organiser.OrganiserID)
            return Forbidden();

        var category = new Category
        {
            EventID = eventId,
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            MinBoatCount = dto.MinBoatCount,
            MaxBoatCount = dto.MaxBoatCount,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { eventId, categoryId = category.CategoryID },
            MapToDto(category));
    }

    /// <summary>
    /// Update a category. Only the owning Organiser.
    /// </summary>
    [HttpPut("{categoryId}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int eventId, int categoryId, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbidden();

        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (eventEntity == null)
            return NotFound(new { message = $"Event with ID {eventId} not found." });

        if (eventEntity.OrganiserID != organiser.OrganiserID)
            return Forbidden();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.EventID == eventId);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        category.CategoryName = dto.CategoryName;
        category.Description = dto.Description;
        category.MinBoatCount = dto.MinBoatCount;
        category.MaxBoatCount = dto.MaxBoatCount;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(category));
    }

    /// <summary>
    /// Delete a category. Only the owning Organiser.
    /// </summary>
    [HttpDelete("{categoryId}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int eventId, int categoryId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbidden();

        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (eventEntity == null)
            return NotFound(new { message = $"Event with ID {eventId} not found." });

        if (eventEntity.OrganiserID != organiser.OrganiserID)
            return Forbidden();

        var category = await _context.Categories
            .Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.EventID == eventId);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        // Remove related enrolments + results first (no cascade)
        var enrolmentIds = category.Enrolments.Select(en => en.EnrolmentID).ToList();
        var results = _context.Results.Where(r => enrolmentIds.Contains(r.EnrolmentID));
        _context.Results.RemoveRange(results);
        _context.EventEnrolments.RemoveRange(category.Enrolments);
        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---------- Helpers ----------
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    private IActionResult Forbidden() =>
        StatusCode(StatusCodes.Status403Forbidden,
            new { message = "You do not have permission to perform this action." });

    private static CategoryResponseDto MapToDto(Category c) => new()
    {
        CategoryID = c.CategoryID,
        CategoryName = c.CategoryName,
        Description = c.Description,
        MinBoatCount = c.MinBoatCount,
        MaxBoatCount = c.MaxBoatCount,
        IsActive = c.IsActive
    };
}