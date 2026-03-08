using FluentValidation;
using AdminService.Application.UseCases.Vehicles.ApproveVehicle;

namespace AdminService.Application.Validators.Vehicles;

/// <summary>
/// Validator for ApproveVehicleCommand.
/// Validates all string fields with NoSqlInjection/NoXss.
/// </summary>
public class ApproveVehicleCommandValidator : AbstractValidator<ApproveVehicleCommand>
{
    public ApproveVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle ID is required.");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty().WithMessage("ApprovedBy is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));

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
