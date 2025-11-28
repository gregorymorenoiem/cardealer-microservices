using FluentValidation;
using NotificationService.Application.UseCases.GetNotificationStatus;

namespace NotificationService.Application.UseCases.GetNotificationStatus;

public class GetNotificationStatusQueryValidator : AbstractValidator<GetNotificationStatusQuery>
{
    public GetNotificationStatusQueryValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("Notification ID is required");
    }
}