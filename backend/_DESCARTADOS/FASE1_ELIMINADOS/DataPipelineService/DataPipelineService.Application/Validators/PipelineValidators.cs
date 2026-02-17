// =====================================================
// DataPipelineService - Validators
// Procesamiento de Datos y ETL
// =====================================================

using FluentValidation;
using DataPipelineService.Application.DTOs;

namespace DataPipelineService.Application.Validators;

public class CreatePipelineValidator : AbstractValidator<CreatePipelineDto>
{
    public CreatePipelineValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("El nombre solo puede contener letras, números, guiones y guiones bajos");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de pipeline inválido");

        RuleFor(x => x.SourceType)
            .NotEmpty().WithMessage("El tipo de fuente es requerido");

        RuleFor(x => x.SourceConfig)
            .NotEmpty().WithMessage("La configuración de fuente es requerida");

        RuleFor(x => x.DestinationType)
            .NotEmpty().WithMessage("El tipo de destino es requerido");

        RuleFor(x => x.DestinationConfig)
            .NotEmpty().WithMessage("La configuración de destino es requerida");

        RuleFor(x => x.CronSchedule)
            .Matches(@"^(\S+\s){4,5}\S+$")
            .When(x => !string.IsNullOrEmpty(x.CronSchedule))
            .WithMessage("Formato de expresión cron inválido");
    }
}

public class CreateStepValidator : AbstractValidator<CreateStepDto>
{
    public CreateStepValidator()
    {
        RuleFor(x => x.PipelineId)
            .NotEmpty().WithMessage("El ID del pipeline es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.StepType)
            .IsInEnum().WithMessage("Tipo de paso inválido");

        RuleFor(x => x.Configuration)
            .NotEmpty().WithMessage("La configuración es requerida");
    }
}

public class CreateConnectorValidator : AbstractValidator<CreateConnectorDto>
{
    public CreateConnectorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.ConnectorType)
            .IsInEnum().WithMessage("Tipo de conector inválido");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("La cadena de conexión es requerida");
    }
}

public class CreateTransformationValidator : AbstractValidator<CreateTransformationDto>
{
    public CreateTransformationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.TransformationType)
            .IsInEnum().WithMessage("Tipo de transformación inválido");

        RuleFor(x => x.SourceQuery)
            .NotEmpty().WithMessage("La consulta fuente es requerida");

        RuleFor(x => x.TransformationLogic)
            .NotEmpty().WithMessage("La lógica de transformación es requerida");

        RuleFor(x => x.TargetTable)
            .NotEmpty().WithMessage("La tabla destino es requerida")
            .MaximumLength(100).WithMessage("El nombre de tabla no puede exceder 100 caracteres");
    }
}

public class StartRunValidator : AbstractValidator<StartRunDto>
{
    public StartRunValidator()
    {
        RuleFor(x => x.PipelineId)
            .NotEmpty().WithMessage("El ID del pipeline es requerido");
    }
}
