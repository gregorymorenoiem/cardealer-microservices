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
        // Input validation
        if (string.IsNullOrWhiteSpace(dto.Subject) || dto.Subject.Length > 200)
            return BadRequest(new { error = "Subject is required and must be 200 characters or less." });
        if (string.IsNullOrWhiteSpace(dto.BuyerName) || dto.BuyerName.Length > 100)
            return BadRequest(new { error = "BuyerName is required and must be 100 characters or less." });
        if (string.IsNullOrWhiteSpace(dto.BuyerEmail) || dto.BuyerEmail.Length > 254)
            return BadRequest(new { error = "BuyerEmail is required and must be a valid email." });
        if (string.IsNullOrWhiteSpace(dto.Message) || dto.Message.Length > 5000)
            return BadRequest(new { error = "Message is required and must be 5000 characters or less." });
        if (dto.VehicleId == Guid.Empty || dto.SellerId == Guid.Empty)
            return BadRequest(new { error = "VehicleId and SellerId are required." });

        // Security validation - check for XSS/SQL injection patterns
        var textFields = new[] { dto.Subject, dto.BuyerName, dto.BuyerEmail, dto.Message, dto.BuyerPhone ?? "" };
        foreach (var field in textFields)
        {
            if (ContainsDangerousPatterns(field))
                return BadRequest(new { error = "Input contains potentially dangerous content." });
        }

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
        try
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
                MessageCount = i.Messages?.Count ?? 0,
                LastMessage = i.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Message
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error retrieving inquiries. Please try again later." });
        }
    }

    /// <summary>
    /// Get contact requests received (seller perspective)
    /// </summary>
    [HttpGet("received")]
    public async Task<IActionResult> GetReceivedInquiries()
    {
        try
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
                MessageCount = i.Messages?.Count ?? 0,
                UnreadCount = i.Messages?.Count(m => m.IsFromBuyer && !m.IsRead) ?? 0
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error retrieving received inquiries. Please try again later." });
        }
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
        if (string.IsNullOrWhiteSpace(dto.Message) || dto.Message.Length > 5000)
            return BadRequest(new { error = "Message is required and must be 5000 characters or less." });
        if (ContainsDangerousPatterns(dto.Message))
            return BadRequest(new { error = "Input contains potentially dangerous content." });

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

    /// <summary>
    /// Mark a contact request as read
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetByIdAsync(id);
        if (contactRequest == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
        {
            return Forbid();
        }

        contactRequest.Status = "Read";
        await _contactRequestRepository.UpdateAsync(contactRequest);

        return NoContent();
    }

    /// <summary>
    /// Archive a contact request
    /// </summary>
    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> ArchiveContactRequest(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetByIdAsync(id);
        if (contactRequest == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
        {
            return Forbid();
        }

        contactRequest.Status = "Archived";
        await _contactRequestRepository.UpdateAsync(contactRequest);

        return NoContent();
    }

    /// <summary>
    /// Delete a contact request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContactRequest(Guid id)
    {
        var contactRequest = await _contactRequestRepository.GetByIdAsync(id);
        if (contactRequest == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (contactRequest.BuyerId != currentUserId && contactRequest.SellerId != currentUserId)
        {
            return Forbid();
        }

        await _contactRequestRepository.DeleteAsync(id);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private static bool ContainsDangerousPatterns(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var lower = input.ToLowerInvariant();
        var upper = input.ToUpperInvariant();

        // XSS patterns
        string[] xssPatterns = { "<script", "javascript:", "onerror=", "onload=", "onclick=",
            "<iframe", "eval(", "expression(", "vbscript:", "data:text/html" };
        if (xssPatterns.Any(p => lower.Contains(p))) return true;

        // SQL injection patterns
        string[] sqlPatterns = { "DROP ", "DELETE ", "INSERT ", "UPDATE ", "--", "/*", "*/",
            "EXEC ", "UNION ", "xp_", "sp_", "OR 1=1", "OR '1'='1'" };
        if (sqlPatterns.Any(p => upper.Contains(p))) return true;

        return false;
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