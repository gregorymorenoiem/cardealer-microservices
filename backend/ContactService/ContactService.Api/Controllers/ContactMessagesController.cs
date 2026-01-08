using ContactService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContactService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactMessagesController : ControllerBase
{
    private readonly IContactMessageRepository _messageRepository;

    public ContactMessagesController(IContactMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _messageRepository.MarkAsReadAsync(id);
        return Ok();
    }

    /// <summary>
    /// Get unread message count for current user
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _messageRepository.GetUnreadCountForUserAsync(userId);
        return Ok(new { Count = count });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}