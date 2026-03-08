using FluentValidation;
using AdminService.Application.UseCases.Vehicles.RejectVehicle;

namespace AdminService.Application.Validators.Vehicles;

/// <summary>
/// Validator for RejectVehicleCommand.
/// Validates all string fields with NoSqlInjection/NoXss.
/// </summary>
public class RejectVehicleCommandValidator : AbstractValidator<RejectVehicleCommand>
{
    public RejectVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle ID is required.");

        RuleFor(x => x.RejectedBy)
            .NotEmpty().WithMessage("RejectedBy is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.OwnerEmail)
            .NotEmpty().WithMessage("Owner email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.VehicleTitle)
            .NotEmpty().WithMessage("Vehicle title is required.")
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss();
    }
}
