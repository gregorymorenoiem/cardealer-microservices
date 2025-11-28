
// WebhooksController.cs
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    [HttpPost("sendgrid")]
    public IActionResult SendGridWebhook()
    {
        // Process SendGrid webhook for email events
        return Ok();
    }

    [HttpPost("twilio")]
    public IActionResult TwilioWebhook()
    {
        // Process Twilio webhook for SMS events
        return Ok();
    }
}