// =====================================================
// VehicleRegistryService - Validators
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using FluentValidation;
using VehicleRegistryService.Application.DTOs;

namespace VehicleRegistryService.Application.Validators;

public class CreateRegistrationValidator : AbstractValidator<CreateRegistrationDto>
{
    public CreateRegistrationValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("La placa es requerida")
            .Matches(@"^[A-Z]\d{6}$").WithMessage("Formato de placa dominicana inválido (ej: A123456)");

        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("El VIN es requerido")
            .Length(17).WithMessage("El VIN debe tener exactamente 17 caracteres");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es requerida");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es requerido");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.Now.Year + 1).WithMessage("Año de fabricación inválido");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color es requerido");

        RuleFor(x => x.EngineNumber)
            .NotEmpty().WithMessage("El número de motor es requerido");

        RuleFor(x => x.ChassisNumber)
            .NotEmpty().WithMessage("El número de chasis es requerido");

        RuleFor(x => x.OwnerIdentification)
            .NotEmpty().WithMessage("La identificación del propietario es requerida")
            .Matches(@"^\d{9,11}$").WithMessage("Cédula o RNC inválido");

        RuleFor(x => x.OwnerName)
            .NotEmpty().WithMessage("El nombre del propietario es requerido");

        RuleFor(x => x.VehicleType)
            .IsInEnum().WithMessage("Tipo de vehículo inválido");

        RuleFor(x => x.FiscalValue)
            .GreaterThan(0).WithMessage("El valor fiscal debe ser mayor a 0");
    }
}

public class CreateTransferValidator : AbstractValidator<CreateTransferDto>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.VehicleRegistrationId)
            .NotEmpty().WithMessage("El ID del registro es requerido");

        RuleFor(x => x.NewOwnerIdentification)
            .NotEmpty().WithMessage("La identificación del nuevo propietario es requerida")
            .Matches(@"^\d{9,11}$").WithMessage("Cédula o RNC inválido");

        RuleFor(x => x.NewOwnerName)
            .NotEmpty().WithMessage("El nombre del nuevo propietario es requerido");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0");
    }
}

public class CreateLienValidator : AbstractValidator<CreateLienDto>
{
    public CreateLienValidator()
    {
        RuleFor(x => x.VehicleRegistrationId)
            .NotEmpty().WithMessage("El ID del registro es requerido");

        RuleFor(x => x.LienHolderName)
            .NotEmpty().WithMessage("El nombre del acreedor es requerido");

        RuleFor(x => x.LienHolderRnc)
            .NotEmpty().WithMessage("El RNC del acreedor es requerido")
            .Matches(@"^\d{9,11}$").WithMessage("RNC inválido");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto del gravamen debe ser mayor a 0");
    }
}

public class ValidateVinValidator : AbstractValidator<ValidateVinDto>
{
    public ValidateVinValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("El VIN es requerido")
            .Length(17).WithMessage("El VIN debe tener exactamente 17 caracteres")
            .Matches(@"^[A-HJ-NPR-Z0-9]{17}$").WithMessage("Formato de VIN inválido (no permite I, O, Q)");
    }
}
