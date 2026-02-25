using FluentValidation;
using UserService.Application.DTOs;

namespace UserService.Application.Validators;

/// <summary>
/// FASE 2: Validadores para campos de Ubicación (Location)
/// City, State, Address, ZipCode
/// </summary>
public class CreateSellerProfileLocationValidator : AbstractValidator<CreateSellerProfileRequest>
{
    public CreateSellerProfileLocationValidator()
    {
        // ========================================
        // VALIDACIONES DE UBICACIÓN (FASE 2)
        // ========================================

        // Lista de provincias válidas de RD
        var validProvinces = new[]
        {
            "Azcona", "Bahoruco", "Barahona", "Distrito Nacional", "Duarte",
            "Elías Piña", "El Seibo", "Espaillat", "Hato Mayor", "Hermanas Mirabal",
            "Independencia", "La Altagracia", "La Romana", "La Vega", "María Trinidad Sánchez",
            "Monseñor Nouel", "Monte Cristi", "Monte Plata", "Pedernales", "Peravia",
            "Puerto Plata", "Salcedo", "Samaná", "Sánchez Ramírez", "San Cristóbal",
            "San José de Ocoa", "San Juan", "San Pedro de Macorís", "Santiago",
            "Santiago Rodríguez", "Santo Domingo", "Valverde"
        };

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("La ciudad es requerida")
            .MinimumLength(2)
            .WithMessage("La ciudad debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("La ciudad no puede exceder 100 caracteres")
            .Must(x => !System.Text.RegularExpressions.Regex.IsMatch(x, @"\d"))
            .WithMessage("La ciudad solo puede contener letras, espacios, guiones y apóstrofes");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("La provincia es requerida")
            .MinimumLength(2)
            .WithMessage("La provincia debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("La provincia no puede exceder 100 caracteres")
            .Must(x => validProvinces.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La provincia no es válida. Debe ser una provincia de República Dominicana");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("La dirección no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.ZipCode)
            .MaximumLength(20)
            .WithMessage("El código postal no puede exceder 20 caracteres")
            .Must(x => !System.Text.RegularExpressions.Regex.IsMatch(x, @"[^a-z0-9\-]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .WithMessage("El código postal solo puede contener números, letras y guiones")
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode));

        // Validación cruzada: Si hay dirección, deben estar city y state
        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    if (string.IsNullOrWhiteSpace(request.City))
                    {
                        context.AddFailure("City", "La ciudad es requerida si se proporciona una dirección");
                    }

                    if (string.IsNullOrWhiteSpace(request.State))
                    {
                        context.AddFailure("State", "La provincia es requerida si se proporciona una dirección");
                    }
                }
            });

        // Validación: Si tiene especialidades, debe tener ubicación válida
        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (request.Specialties != null && request.Specialties.Length > 0)
                {
                    if (string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.State))
                    {
                        context.AddFailure("Specialties",
                            "Debes especificar tu ubicación (ciudad y provincia) para indicar tus especialidades");
                    }
                }
            });
    }
}

/// <summary>
/// FASE 2: Validador para actualizar Location en UpdateSellerProfileRequest
/// </summary>
public class UpdateSellerProfileLocationValidator : AbstractValidator<UpdateSellerProfileRequest>
{
    public UpdateSellerProfileLocationValidator()
    {
        var validProvinces = new[]
        {
            "Azcona", "Bahoruco", "Barahona", "Distrito Nacional", "Duarte",
            "Elías Piña", "El Seibo", "Espaillat", "Hato Mayor", "Hermanas Mirabal",
            "Independencia", "La Altagracia", "La Romana", "La Vega", "María Trinidad Sánchez",
            "Monseñor Nouel", "Monte Cristi", "Monte Plata", "Pedernales", "Peravia",
            "Puerto Plata", "Salcedo", "Samaná", "Sánchez Ramírez", "San Cristóbal",
            "San José de Ocoa", "San Juan", "San Pedro de Macorís", "Santiago",
            "Santiago Rodríguez", "Santo Domingo", "Valverde"
        };

        RuleFor(x => x.City)
            .MinimumLength(2)
            .WithMessage("La ciudad debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("La ciudad no puede exceder 100 caracteres")
            .Must(x => !System.Text.RegularExpressions.Regex.IsMatch(x, @"\d"))
            .WithMessage("La ciudad solo puede contener letras, espacios, guiones y apóstrofes")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.State)
            .MinimumLength(2)
            .WithMessage("La provincia debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("La provincia no puede exceder 100 caracteres")
            .Must(x => validProvinces.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("La provincia no es válida. Debe ser una provincia de República Dominicana")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("La dirección no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.ZipCode)
            .MaximumLength(20)
            .WithMessage("El código postal no puede exceder 20 caracteres")
            .Must(x => !System.Text.RegularExpressions.Regex.IsMatch(x, @"[^a-z0-9\-]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .WithMessage("El código postal solo puede contener números, letras y guiones")
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode));

        // Validación: Si actualiza dirección, debe incluir city y state
        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    if (string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.State))
                    {
                        context.AddFailure("Location",
                            "Al actualizar la dirección, debes incluir city y state");
                    }
                }
            });
    }
}
