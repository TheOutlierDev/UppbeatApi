using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UppbeatApi.Data.Models;
using UppbeatApi.Interfaces;

namespace UppbeatApi.Controllers;

/// <summary>
/// Controller for managing uppbeat tracks in the library
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TrackController : ControllerBase
{
    private readonly ITrackRepository _trackRepository;

    public TrackController(ITrackRepository trackRepository)
    {
        _trackRepository = trackRepository;
    }

    /// <summary>
    /// Adds a new track to the uppbeat library
    /// </summary>
    /// <param name="track">The track information to add</param>
    /// <returns>The created track with its assigned ID</returns>
    /// <response code="201">Returns the newly created track</response>
    /// <response code="400">If the track data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized as an Artist</response>
    [HttpPost]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Track>> AddTrack([FromBody, Required] Track track)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (User?.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You must be logged in as an Artist to add tracks.");
        }

        var createdTrack = await _trackRepository.AddTrackAsync(track);
        return CreatedAtAction(nameof(GetTrackById), new { id = createdTrack.Id }, createdTrack);
    }

    /// <summary>
    /// Retrieves a list of tracks from the uppbeat library
    /// </summary>
    /// <param name="genre">Optional genre filter</param>
    /// <param name="search">Optional search query</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of tracks per page</param>
    /// <returns>A list of tracks matching the filter criteria</returns>
    /// <response code="200">Returns the list of tracks</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Track>>> GetTracks(
        [FromQuery] string? genre = null,
        [FromQuery] string? search = null,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        var (tracks, totalCount) = await _trackRepository.GetTracksAsync(genre, search, page, pageSize);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(tracks);
    }

    /// <summary>
    /// Retrieves a track by its ID
    /// </summary>
    /// <param name="id">The ID of the track to retrieve</param>
    /// <returns>The track with the specified ID</returns>
    /// <response code="200">Returns the track</response>
    /// <response code="404">If the track is not found</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Track>> GetTrackById([FromRoute] Guid id)
    {
        var track = await _trackRepository.GetTrackByIdAsync(id);
        if (track == null)
        {
            return NotFound();
        }
        return Ok(track);
    }

    /// <summary>
    /// Updates a track in the uppbeat library
    /// </summary>
    /// <param name="id">The ID of the track to update</param>
    /// <param name="updatedTrack">The updated track information</param>
    /// <returns>No content</returns>
    /// <response code="204">Track updated successfully</response>
    /// <response code="400">If the track data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized as an Artist</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTrack([FromRoute] Guid id, [FromBody, Required] Track updatedTrack)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (User?.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You must be logged in as an Artist to update tracks.");
        }

        var track = await _trackRepository.GetTrackByIdAsync(id);
        if (track == null)
        {
            return NotFound();
        }

        await _trackRepository.UpdateTrackAsync(id, updatedTrack);
        return NoContent();
    }

    /// <summary>
    /// Deletes a track from the uppbeat library
    /// </summary>
    /// <param name="id">The ID of the track to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Track deleted successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized as an Artist</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTrack([FromRoute] Guid id)
    {
        if (User?.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You must be logged in as an Artist to delete tracks.");
        }

        var track = await _trackRepository.GetTrackByIdAsync(id);
        if (track == null)
        {
            return NotFound();
        }

        await _trackRepository.DeleteTrackAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Downloads a track from the uppbeat library
    /// </summary>
    /// <param name="id">The ID of the track to download</param>
    /// <returns>The track file</returns>
    /// <response code="200">Returns the track file</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized as Regular or Artist</response>
    [HttpGet("{id}/download")]
    [Authorize(Roles = "Regular,Artist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadTrack([FromRoute] Guid id)
    {
        if (User?.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You must be logged in to download tracks.");
        }

        if (!User.IsInRole("Artist") && !User.IsInRole("Regular"))
        {
            return Forbid("You must be an Artist or Regular user to download tracks.");
        }

        var track = await _trackRepository.GetTrackByIdAsync(id);
        if (track == null)
        {
            return NotFound();
        }

        // Simulate downloading the track file
        var fileBytes = Encoding.UTF8.GetBytes(track.File ?? string.Empty);
        var fileName = track.Name + ".mp3";

        return File(fileBytes, "application/octet-stream", fileName);
    }
}
