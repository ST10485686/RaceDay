using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Result;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all results for a specific event (public).
    /// </summary>
    [HttpGet("events/{eventId}/results")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ResultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.EventID == eventId);
        if (!exists)
            return NotFound(new { message = $"Event with ID {eventId} not found." });

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Participant).ThenInclude(p => p.User)
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .Where(r => r.Enrolment.Category.EventID == eventId)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Get all results for the currently logged-in participant.
    /// </summary>
    [HttpGet("users/me/results")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(List<ResultResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId.Value);
        if (participant == null) return Forbid();

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Participant).ThenInclude(p => p.User)
            .Include(r => r.Enrolment).ThenInclude(en => en.Category).ThenInclude(c => c.Event)
            .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Record or update a result for an enrolment. Only the owning Organiser.
    /// </summary>
    [HttpPost("enrolments/{enrolmentId}/results")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(ResultResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrUpdate(int enrolmentId, [FromBody] CreateResultDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId.Value);
        if (organiser == null) return Forbid();

        var enrolment = await _context.EventEnrolments
            .Include(en => en.Category).ThenInclude(c => c.Event)
            .Include(en => en.Participant).ThenInclude(p => p.User)
            .Include(en => en.Result)
            .FirstOrDefaultAsync(en => en.EnrolmentID == enrolmentId);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        // If you have not implemented the rest of the method yet, return a NotImplemented result for now:
        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "CreateOrUpdate logic not implemented." });
    }

    /// <summary>
    /// Helper to get current user ID from claims.
    /// </summary>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return null;
        if (int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }

    /// <summary>
    /// Maps a Result entity to a ResultResponseDto.
    /// </summary>
    private static ResultResponseDto MapToDto(Result result)
    {
        return new ResultResponseDto
        {
            ResultID = result.ResultID,
            EnrolmentID = result.EnrolmentID,
            Position = result.Position,
            FinalTime = result.FinalTime,
            Points = result.Points,
            Penalties = result.Penalties,
            IsVerified = result.IsVerified,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt,
            // Map nested properties as needed, e.g.:
            ParticipantName = result.Enrolment?.Participant?.User?.FirstName + " " + result.Enrolment?.Participant?.User?.LastName,
            CategoryName = result.Enrolment?.Category?.CategoryName,
            EventTitle = result.Enrolment?.Category?.Event?.Title
        };
    }
}