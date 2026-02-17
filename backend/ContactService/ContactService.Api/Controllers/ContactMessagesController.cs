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
    private readonly IContactRequestRepository _contactRequestRepository;

    public ContactMessagesController(
        IContactMessageRepository messageRepository,
        IContactRequestRepository contactRequestRepository)
    {
        _messageRepository = messageRepository;
        _contactRequestRepository = contactRequestRepository;
    }

    /// <summary>
    /// Mark a message as read (only the message recipient can mark it)
    /// </summary>
    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { error = "Message not found" });

        // Verify the current user is the recipient (not the sender)
        var currentUserId = GetCurrentUserId();
        var contactRequest = await _contactRequestRepository.GetByIdAsync(message.ContactRequestId);
        if (contactRequest == null)
            return NotFound(new { error = "Contact request not found" });

        // Only the other party (not the sender) should mark messages as read
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
            return Forbid();

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