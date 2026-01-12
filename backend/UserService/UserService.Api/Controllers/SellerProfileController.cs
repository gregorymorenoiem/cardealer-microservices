using Microsoft.AspNetCore.Mvc;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellerProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public SellerProfileController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get public seller profile information
    /// </summary>
    [HttpGet("{sellerId}")]
    public async Task<IActionResult> GetSellerProfile(Guid sellerId)
    {
        var user = await _userRepository.GetByIdAsync(sellerId);
        if (user == null) return NotFound();

        // Return only public information
        return Ok(new
        {
            Id = user.Id,
            FullName = user.FullName,
            AccountType = user.AccountType,
            ProfilePicture = user.ProfilePicture,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Province = user.Province,
            MemberSince = user.CreatedAt,
            IsVerified = user.IsEmailVerified,
            DealerInfo = user.AccountType == AccountType.Dealer ? new
            {
                BusinessName = user.BusinessName,
                BusinessPhone = user.BusinessPhone,
                BusinessAddress = user.BusinessAddress,
                RNC = user.RNC
            } : null,
            Stats = new
            {
                TotalListings = 0, // TODO: Get from VehiclesSaleService
                ActiveListings = 0, // TODO: Get from VehiclesSaleService
                ResponseRate = "95%", // TODO: Calculate from ContactService
                ResponseTime = "Within 2 hours" // TODO: Calculate from ContactService
            }
        });
    }

    /// <summary>
    /// Get seller contact preferences
    /// </summary>
    [HttpGet("{sellerId}/contact-preferences")]
    public async Task<IActionResult> GetSellerContactPreferences(Guid sellerId)
    {
        var user = await _userRepository.GetByIdAsync(sellerId);
        if (user == null) return NotFound();

        return Ok(new
        {
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            PreferredContactMethod = user.PreferredContactMethod ?? "email",
            BusinessHours = user.BusinessHours ?? "9:00 AM - 6:00 PM",
            AutoReplyMessage = user.AutoReplyMessage
        });
    }
}