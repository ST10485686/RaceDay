using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Event;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all events (public). Includes optional filters by status and event type.
    /// </summary>
    /// <param name="status">Optional: Draft, Open, InProgress, Completed, Cancelled</param>
    /// <param name="eventType">Optional: Race, Regatta, Series</param>
    /// <response code="200">Returns the list of events.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<EventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? eventType)
    {
        var query = _context.Events
            .Include(e => e.Organiser)
            .ThenInclude(o => o.User)
            .Include(e => e.Categories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);

        var events = await query
            .OrderByDescending(e => e.DateStart)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>
    /// Get a single event by ID with its categories.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <response code="200">Returns the event.</response>
    /// <response code="404">If the event does not exist.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Organiser)
            .ThenInclude(o => o.User)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (ev == null)
            return NotFound(new { message = $"Event with ID {id} not found." });

        return Ok(MapToDto(ev));
    }

    /// <summary>
    /// Create a new event. Only accessible to Organisers.
    /// </summary>
    /// <param name="dto">Event creation payload.</param>
    /// <response code="201">Event created successfully.</response>
    /// <response code="400">Validation error or invalid date range.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.DateEnd < dto.DateStart)
            return BadRequest(new { message = "End date must be after start date." });

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId.Value);

        if (organiser == null)
            return Forbidden();

        var newEvent = new Event
        {
            OrganiserID = organiser.OrganiserID,
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            DateStart = dto.DateStart,
            DateEnd = dto.DateEnd,
            Status = dto.Status,
            MaxParticipants = dto.MaxParticipants,
            EventType = dto.EventType,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        // Reload with navigation props
        var created = await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .Include(e => e.Categories)
            .FirstAsync(e => e.EventID == newEvent.EventID);

        return CreatedAtAction(nameof(GetById), new { id = created.EventID }, MapToDto(created));
    }

    /// <summary>
    /// Update an existing event. Only the owning Organiser can update it.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <param name="dto">Updated event fields.</param>
    /// <response code="200">Event updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Not the event's owner.</response>
    /// <response code="404">Event not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.DateEnd < dto.DateStart)
            return BadRequest(new { message = "End date must be after start date." });

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbidden();

        var existing = await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (existing == null)
            return NotFound(new { message = $"Event with ID {id} not found." });

        if (existing.OrganiserID != organiser.OrganiserID)
            return Forbidden();

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Location = dto.Location;
        existing.DateStart = dto.DateStart;
        existing.DateEnd = dto.DateEnd;
        existing.Status = dto.Status;
        existing.MaxParticipants = dto.MaxParticipants;
        existing.EventType = dto.EventType;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(existing));
    }

    /// <summary>
    /// Delete an event. Only the owning Organiser can delete it.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <response code="204">Event deleted successfully.</response>
    /// <response code="403">Not the event's owner.</response>
    /// <response code="404">Event not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbidden();

        var ev = await _context.Events
            .Include(e => e.Categories)
            .ThenInclude(c => c.Enrolments)
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (ev == null)
            return NotFound(new { message = $"Event with ID {id} not found." });

        if (ev.OrganiserID != organiser.OrganiserID)
            return Forbidden();

        // Manually remove related enrolments (since we disabled cascade)
        var enrolmentIds = ev.Categories.SelectMany(c => c.Enrolments).Select(en => en.EnrolmentID).ToList();
        var results = _context.Results.Where(r => enrolmentIds.Contains(r.EnrolmentID));
        _context.Results.RemoveRange(results);
        _context.EventEnrolments.RemoveRange(ev.Categories.SelectMany(c => c.Enrolments));
        _context.Categories.RemoveRange(ev.Categories);
        _context.Events.Remove(ev);

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

    private static EventResponseDto MapToDto(Event e) => new()
    {
        EventID = e.EventID,
        Title = e.Title,
        Description = e.Description,
        Location = e.Location,
        DateStart = e.DateStart,
        DateEnd = e.DateEnd,
        Status = e.Status,
        MaxParticipants = e.MaxParticipants,
        EventType = e.EventType,
        OrganiserID = e.OrganiserID,
        OrganiserName = e.Organiser?.OrganisationName ?? "Unknown",
        CreatedAt = e.CreatedAt,
        Categories = e.Categories?.Select(c => new CategoryResponseDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            Description = c.Description,
            MinBoatCount = c.MinBoatCount,
            MaxBoatCount = c.MaxBoatCount,
            IsActive = c.IsActive
        }).ToList() ?? new List<CategoryResponseDto>()
    };
}