using System.Text.RegularExpressions;
using ArchiTrackerBE.Data;
using ArchiTrackerBE.Dtos;
using ArchiTrackerBE.Models;
using ArchiTrackerBE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArchiTrackerBE.Controllers;

[ApiController]
[Route("api/archipelago-room")]
public class ArchipelagoRoomController(AppDbContext context, ArchipelagoTrackerService trackerService) : ControllerBase
{
    private static readonly Regex RoomCodeRegex = new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);
    private readonly ArchipelagoTrackerService _trackerService = trackerService;

    [HttpPost]
    [ProducesResponseType(typeof(ArchipelagoRoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertAsync([FromBody] ArchipelagoRoomRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "Url is required." });
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            return BadRequest(new { error = "Url must be a valid absolute URI." });
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Url must use https scheme." });
        }

        if (!string.Equals(uri.Host, "archipelago.gg", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only archipelago.gg URLs are accepted." });
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2 || !string.Equals(segments[0], "room", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Url must target a room (e.g. https://archipelago.gg/room/XXXX)." });
        }

        var roomCode = segments[1];
        if (!RoomCodeRegex.IsMatch(roomCode))
        {
            return BadRequest(new { error = "Room identifier contains invalid characters." });
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(clientIp) && Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            clientIp = forwarded.FirstOrDefault()?.Split(',')[0].Trim();
        }

        var existing = await context.ArchipelagoRooms
            .FirstOrDefaultAsync(room => room.Link == roomCode, cancellationToken);

        if (existing is null)
        {
            existing = new ArchipelagoRoom
            {
                Url = request.Url,
                Link = roomCode,
                IpAdded = clientIp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            context.ArchipelagoRooms.Add(existing);
        }
        else
        {
            existing.Url = request.Url;
            existing.IpAdded = clientIp;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Ok(new ArchipelagoRoomResponse
        {
            Id = existing.Id,
            Url = existing.Url,
            Link = existing.Link,
            IpAdded = existing.IpAdded,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt,
        });
    }

    [HttpGet("{roomCode}")]
    [ProducesResponseType(typeof(ArchipelagoRoomVisualizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomAsync(string roomCode, CancellationToken cancellationToken)
    {
        var normalized = roomCode.Trim();
        var room = await context.ArchipelagoRooms.FirstOrDefaultAsync(r => r.Link == normalized, cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        try
        {
            var result = await _trackerService.GetRoomDetailsAsync(room, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Unable to reach Archipelago tracker." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "Tracker request timed out." });
        }
    }
}
