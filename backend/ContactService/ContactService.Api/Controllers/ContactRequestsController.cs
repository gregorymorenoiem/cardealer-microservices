using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContactService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactRequestsController : ControllerBase
{
    private readonly IContactRequestRepository _contactRequestRepository;
    private readonly IContactMessageRepository _contactMessageRepository;

    public ContactRequestsController(IContactRequestRepository contactRequestRepository, 
                                   IContactMessageRepository contactMessageRepository)
    {
        _contactRequestRepository = contactRequestRepository;
        _contactMessageRepository = contactMessageRepository;
    }

    /// <summary>
    /// Create a new contact request (inquiry)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContactRequest(CreateContactRequestDto dto)
    {
        var buyerId = GetCurrentUserId();
        
        var contactRequest = new ContactRequest(
            dto.VehicleId,
            buyerId,
            dto.SellerId,
            dto.Subject,
            dto.BuyerName,
            dto.BuyerEmail,
            dto.Message
        );
        
        contactRequest.BuyerPhone = dto.BuyerPhone;

        var created = await _contactRequestRepository.CreateAsync(contactRequest);
        
        // Create initial message
        var initialMessage = new ContactMessage(created.Id, buyerId, dto.Message, true);
        await _contactMessageRepository.CreateAsync(initialMessage);

        return Ok(new
        {
            Id = created.Id,
            VehicleId = created.VehicleId,
            Subject = created.Subject,
            Status = created.Status,
            CreatedAt = created.CreatedAt
        });
    }

    /// <summary>
    /// Get contact requests for the current user (buyer perspective)
    /// </summary>
    [HttpGet("my-inquiries")]
    public async Task<IActionResult> GetMyInquiries()
    {
        var buyerId = GetCurrentUserId();
        var inquiries = await _contactRequestRepository.GetByBuyerIdAsync(buyerId);

        return Ok(inquiries.Select(i => new
        {
            Id = i.Id,
            VehicleId = i.VehicleId,
            Subject = i.Subject,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            RespondedAt = i.RespondedAt,
            MessageCount = i.Messages.Count,
            LastMessage = i.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Message
        }));
    }

    /// <summary>
    /// Get contact requests received (seller perspective)
    /// </summary>
    [HttpGet("received")]
    public async Task<IActionResult> GetReceivedInquiries()
    {
        var sellerId = GetCurrentUserId();
        var inquiries = await _contactRequestRepository.GetBySellerIdAsync(sellerId);

        return Ok(inquiries.Select(i => new
        {
            Id = i.Id,
            VehicleId = i.VehicleId,
            Subject = i.Subject,
            BuyerName = i.BuyerName,
            BuyerEmail = i.BuyerEmail,
            BuyerPhone = i.BuyerPhone,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            RespondedAt = i.RespondedAt,
            MessageCount = i.Messages.Count,
            UnreadCount = i.Messages.Count(m => m.IsFromBuyer && !m.IsRead)
        }));
    }

    /// <summary>
    /// Get specific contact request with messages
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContactRequest(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetByIdAsync(id);
        if (contactRequest == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
        {
            return Forbid();
        }

        var messages = await _contactMessageRepository.GetByContactRequestIdAsync(id);

        return Ok(new
        {
            Id = contactRequest.Id,
            VehicleId = contactRequest.VehicleId,
            Subject = contactRequest.Subject,
            BuyerName = contactRequest.BuyerName,
            BuyerEmail = contactRequest.BuyerEmail,
            BuyerPhone = contactRequest.BuyerPhone,
            Status = contactRequest.Status,
            CreatedAt = contactRequest.CreatedAt,
            Messages = messages.Select(m => new
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Message = m.Message,
                IsFromBuyer = m.IsFromBuyer,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            })
        });
    }

    /// <summary>
    /// Reply to a contact request
    /// </summary>
    [HttpPost("{id}/reply")]
    public async Task<IActionResult> ReplyToContactRequest(Guid id, ReplyToContactRequestDto dto)
    {
        var contactRequest = await _contactRequestRepository.GetByIdAsync(id);
        if (contactRequest == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
        {
            return Forbid();
        }

        var isFromBuyer = contactRequest.BuyerId == currentUserId;
        
        var message = new ContactMessage(id, currentUserId, dto.Message, isFromBuyer);
        await _contactMessageRepository.CreateAsync(message);

        // Update contact request status
        if (!isFromBuyer && contactRequest.Status == "Open")
        {
            contactRequest.Status = "Responded";
            contactRequest.RespondedAt = DateTime.UtcNow;
            await _contactRequestRepository.UpdateAsync(contactRequest);
        }

        return Ok(new
        {
            Id = message.Id,
            Message = message.Message,
            SentAt = message.SentAt
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}

public record CreateContactRequestDto(
    Guid VehicleId,
    Guid SellerId,
    string Subject,
    string BuyerName,
    string BuyerEmail,
    string? BuyerPhone,
    string Message
);

public record ReplyToContactRequestDto(string Message);