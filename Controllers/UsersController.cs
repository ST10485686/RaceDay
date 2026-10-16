using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs.User;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get the profile of the currently logged-in user.
    /// </summary>
    /// <returns>User profile details based on role (Organiser or Participant).</returns>
    /// <response code="200">Returns the user's profile.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid or missing user identity." });

        var profile = await _userService.GetProfileAsync(userId.Value);
        if (profile == null)
            return NotFound(new { message = "User not found." });

        return Ok(profile);
    }

    /// <summary>
    /// Update the profile of the currently logged-in user.
    /// </summary>
    /// <param name="dto">Profile fields to update.</param>
    /// <returns>The updated user profile.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(new { message = "Invalid or missing user identity." });

        var updated = await _userService.UpdateProfileAsync(userId.Value, dto);
        if (updated == null)
            return NotFound(new { message = "User not found." });

        return Ok(updated);
    }

    /// <summary>
    /// Helper: Extract the current user's ID from the JWT claims.
    /// </summary>
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}