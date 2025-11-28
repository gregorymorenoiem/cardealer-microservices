// NotificationService.Api\Controllers\NotificationsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.UseCases.SendEmailNotification;
using NotificationService.Application.UseCases.SendSmsNotification;
using NotificationService.Application.UseCases.SendPushNotification;
using NotificationService.Application.UseCases.GetNotificationStatus;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("email")]
    public async Task<ActionResult<SendEmailNotificationResponse>> SendEmail([FromBody] SendEmailNotificationRequest request)
    {
        var command = new SendEmailNotificationCommand(request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("sms")]
    public async Task<ActionResult<SendSmsNotificationResponse>> SendSms([FromBody] SendSmsNotificationRequest request)
    {
        var command = new SendSmsNotificationCommand(request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("push")]
    public async Task<ActionResult<SendPushNotificationResponse>> SendPush([FromBody] SendPushNotificationRequest request)
    {
        var command = new SendPushNotificationCommand(request);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id:guid}/status")]
    public async Task<ActionResult<GetNotificationStatusResponse>> GetStatus(Guid id)
    {
        // ✅ Usar el namespace completo para evitar ambigüedad
        var query = new Application.UseCases.GetNotificationStatus.GetNotificationStatusQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}