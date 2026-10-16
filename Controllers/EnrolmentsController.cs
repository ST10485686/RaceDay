using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Enrolment;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all enrolments for a specific event (Organiser view).
    /// </summary>
    [HttpGet("~/api/events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEvent(int eventId)
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

        var enrolments = await _context.EventEnrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Where(en => en.Category.EventID == eventId)
            .Select(en => MapToDto(en))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>
    /// Get all enrolments for the currently logged-in participant.
    /// </summary>
    [HttpGet("~/api/users/me/enrolments")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId.Value);
        if (participant == null) return Forbidden();

        var enrolments = await _context.EventEnrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Where(en => en.ParticipantID == participant.ParticipantID)
            .Select(en => MapToDto(en))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>
    /// Enrol the logged-in participant in a category of an event.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEnrolmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId.Value);
        if (participant == null) return Forbidden();

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.EventID != dto.EventID)
            return BadRequest(new { message = "Category does not belong to the specified event." });

        if (category.Event.Status != "Open")
            return BadRequest(new { message = "Event is not open for enrolment." });

        // Check duplicate enrolment
        var alreadyEnrolled = await _context.EventEnrolments
            .AnyAsync(en => en.ParticipantID == participant.ParticipantID
                         && en.CategoryID == dto.CategoryID);

        if (alreadyEnrolled)
            return Conflict(new { message = "You are already enrolled in this category." });

        var enrolment = new EventEnrolment
        {
            ParticipantID = participant.ParticipantID,
            CategoryID = dto.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            Notes = dto.Notes
        };

        _context.EventEnrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        // Reload with includes
        var created = await _context.EventEnrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .FirstAsync(en => en.EnrolmentID == enrolment.EnrolmentID);

        return CreatedAtAction(nameof(GetMyEnrolments), null, MapToDto(created));
    }

    /// <summary>
    /// Update the status of an enrolment. Organiser or owning Participant.
    /// </summary>
    [HttpPut("{enrolmentId}")]
    [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int enrolmentId, [FromBody] UpdateEnrolmentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var enrolment = await _context.EventEnrolments
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(en => en.EnrolmentID == enrolmentId);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isOwnerParticipant = enrolment.Participant.UserID == userId.Value;
        bool isEventOrganiser = false;

        if (role == "Organiser")
        {
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
            isEventOrganiser = organiser != null &&
                               enrolment.Category.Event.OrganiserID == organiser.OrganiserID;
        }

        if (!isOwnerParticipant && !isEventOrganiser)
            return Forbidden();

        if (!string.IsNullOrEmpty(dto.Status))
            enrolment.Status = dto.Status;

        if (!string.IsNullOrEmpty(dto.PaymentStatus))
            enrolment.PaymentStatus = dto.PaymentStatus;

        if (dto.Notes != null)
            enrolment.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(enrolment));
    }

    /// <summary>
    /// Withdraw the logged-in participant's enrolment.
    /// </summary>
    [HttpDelete("{enrolmentId}")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(int enrolmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId.Value);
        if (participant == null) return Forbidden();

        var enrolment = await _context.EventEnrolments
            .FirstOrDefaultAsync(en => en.EnrolmentID == enrolmentId);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        if (enrolment.ParticipantID != participant.ParticipantID)
            return Forbidden();

        // Remove any existing result first
        var result = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);
        if (result != null)
            _context.Results.Remove(result);

        _context.EventEnrolments.Remove(enrolment);
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

    private static EnrolmentResponseDto MapToDto(EventEnrolment en) => new()
    {
        EnrolmentID = en.EnrolmentID,
        EventID = en.Category?.EventID ?? 0,
        EventTitle = en.Category?.Event?.Title ?? "Unknown Event",
        CategoryID = en.CategoryID,
        CategoryName = en.Category?.CategoryName ?? "Unknown",
        ParticipantID = en.ParticipantID,
        ParticipantName = en.Participant?.User != null
            ? $"{en.Participant.User.FirstName} {en.Participant.User.LastName}"
            : "Unknown",
        EnrolmentDate = en.EnrolmentDate,
        Status = en.Status,
        PaymentStatus = en.PaymentStatus,
        Notes = en.Notes
    };
}