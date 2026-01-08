using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PostgresDbService.Api.Controllers;

/// <summary>
/// Contact-specific controller with type-safe operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactRepository _contactRepository;
    private readonly ILogger&lt;ContactsController&gt; _logger;

    public ContactsController(IContactRepository contactRepository, ILogger&lt;ContactsController&gt; logger)
    {
        _contactRepository = contactRepository;
        _logger = logger;
    }

    /// &lt;summary&gt;
    /// Get contact request by ID
    /// &lt;/summary&gt;
    [HttpGet("requests/{contactRequestId:guid}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; GetRequestById(Guid contactRequestId)
    {
        try
        {
            var request = await _contactRepository.GetContactRequestByIdAsync(contactRequestId);
            if (request == null)
                return NotFound($"Contact request not found: {contactRequestId}");

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact request: {ContactRequestId}", contactRequestId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get contact requests by buyer
    /// &lt;/summary&gt;
    [HttpGet("requests/by-buyer/{buyerId:guid}")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetRequestsByBuyer(Guid buyerId)
    {
        try
        {
            var requests = await _contactRepository.GetContactRequestsByBuyerAsync(buyerId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact requests by buyer: {BuyerId}", buyerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get contact requests by seller
    /// &lt;/summary&gt;
    [HttpGet("requests/by-seller/{sellerId:guid}")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetRequestsBySeller(Guid sellerId)
    {
        try
        {
            var requests = await _contactRepository.GetContactRequestsBySellerAsync(sellerId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact requests by seller: {SellerId}", sellerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get messages for a contact request
    /// &lt;/summary&gt;
    [HttpGet("requests/{contactRequestId:guid}/messages")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetMessagesByRequest(Guid contactRequestId)
    {
        try
        {
            var messages = await _contactRepository.GetContactMessagesByRequestAsync(contactRequestId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for contact request: {ContactRequestId}", contactRequestId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Create a new contact request
    /// &lt;/summary&gt;
    [HttpPost("requests")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; CreateRequest([FromBody] CreateContactRequestRequest request)
    {
        try
        {
            var contactData = new
            {
                BuyerId = request.BuyerId,
                SellerId = request.SellerId,
                VehicleId = request.VehicleId,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                BuyerPhone = request.BuyerPhone,
                Message = request.Message,
                IsUrgent = request.IsUrgent,
                PreferredContactMethod = request.PreferredContactMethod,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            var contactRequest = await _contactRepository.CreateContactRequestAsync(contactData, User.Identity?.Name ?? "system");
            
            return CreatedAtAction(nameof(GetRequestById), 
                new { contactRequestId = Guid.Parse(contactRequest.EntityId) }, 
                contactRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact request");
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Create a new message for a contact request
    /// &lt;/summary&gt;
    [HttpPost("requests/{contactRequestId:guid}/messages")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; CreateMessage(
        Guid contactRequestId, 
        [FromBody] CreateContactMessageRequest request)
    {
        try
        {
            // Verify contact request exists
            var contactRequest = await _contactRepository.GetContactRequestByIdAsync(contactRequestId);
            if (contactRequest == null)
                return NotFound($"Contact request not found: {contactRequestId}");

            var messageData = new
            {
                ContactRequestId = contactRequestId.ToString(),
                SenderId = request.SenderId,
                SenderName = request.SenderName,
                Message = request.Message,
                MessageType = request.MessageType ?? "Text",
                IsFromBuyer = request.IsFromBuyer,
                ReadAt = (DateTime?)null,
                CreatedAt = DateTime.UtcNow
            };

            var message = await _contactRepository.CreateContactMessageAsync(messageData, User.Identity?.Name ?? "system");
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact message for request: {ContactRequestId}", contactRequestId);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// &lt;summary&gt;
/// Request model for creating contact requests
/// &lt;/summary&gt;
public record CreateContactRequestRequest(
    [Required] string BuyerId,
    [Required] string SellerId,
    [Required] string VehicleId,
    [Required] string BuyerName,
    [Required] string BuyerEmail,
    string? BuyerPhone,
    [Required] string Message,
    bool IsUrgent = false,
    string PreferredContactMethod = "Email"
);

/// &lt;summary&gt;
/// Request model for creating contact messages
/// &lt;/summary&gt;
public record CreateContactMessageRequest(
    [Required] string SenderId,
    [Required] string SenderName,
    [Required] string Message,
    string? MessageType = "Text",
    bool IsFromBuyer = true
);