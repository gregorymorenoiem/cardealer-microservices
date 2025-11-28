using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.SendPushNotification;

public class SendPushNotificationCommandValidator : AbstractValidator<SendPushNotificationCommand>
{
    public SendPushNotificationCommandValidator()
    {
        RuleFor(x => x.Request.DeviceToken)
            .NotEmpty().WithMessage("Device token is required");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.Request.Body)
            .NotEmpty().WithMessage("Body is required")
            .MaximumLength(200).WithMessage("Body cannot exceed 200 characters");
    }
}