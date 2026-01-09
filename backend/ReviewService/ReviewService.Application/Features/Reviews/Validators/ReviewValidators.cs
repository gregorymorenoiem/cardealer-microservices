using FluentValidation;
using ReviewService.Application.Features.Reviews.Commands;

namespace ReviewService.Application.Features.Reviews.Validators;

/// &lt;summary&gt;
/// Validador para CreateReviewCommand
/// &lt;/summary&gt;
public class CreateReviewCommandValidator : AbstractValidator&lt;CreateReviewCommand&gt;
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x =&gt; x.BuyerId)
            .NotEmpty()
            .WithMessage("BuyerId es requerido");

        RuleFor(x =&gt; x.SellerId)
            .NotEmpty()
            .WithMessage("SellerId es requerido");

        RuleFor(x =&gt; x.BuyerId)
            .NotEqual(x =&gt; x.SellerId)
            .WithMessage("No puedes dejarte una review a ti mismo");

        RuleFor(x =&gt; x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("El rating debe estar entre 1 y 5 estrellas");

        RuleFor(x =&gt; x.Title)
            .NotEmpty()
            .WithMessage("El título es requerido")
            .MaximumLength(200)
            .WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x =&gt; x.Content)
            .NotEmpty()
            .WithMessage("El contenido es requerido")
            .MinimumLength(10)
            .WithMessage("El contenido debe tener al menos 10 caracteres")
            .MaximumLength(2000)
            .WithMessage("El contenido no puede exceder 2000 caracteres");

        RuleFor(x =&gt; x.BuyerName)
            .NotEmpty()
            .WithMessage("El nombre del comprador es requerido")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres");
    }
}

/// &lt;summary&gt;
/// Validador para UpdateReviewCommand
/// &lt;/summary&gt;
public class UpdateReviewCommandValidator : AbstractValidator&lt;UpdateReviewCommand&gt;
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x =&gt; x.ReviewId)
            .NotEmpty()
            .WithMessage("ReviewId es requerido");

        RuleFor(x =&gt; x.BuyerId)
            .NotEmpty()
            .WithMessage("BuyerId es requerido");

        RuleFor(x =&gt; x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("El rating debe estar entre 1 y 5 estrellas");

        RuleFor(x =&gt; x.Title)
            .NotEmpty()
            .WithMessage("El título es requerido")
            .MaximumLength(200)
            .WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x =&gt; x.Content)
            .NotEmpty()
            .WithMessage("El contenido es requerido")
            .MinimumLength(10)
            .WithMessage("El contenido debe tener al menos 10 caracteres")
            .MaximumLength(2000)
            .WithMessage("El contenido no puede exceder 2000 caracteres");
    }
}

/// &lt;summary&gt;
/// Validador para ModerateReviewCommand
/// &lt;/summary&gt;
public class ModerateReviewCommandValidator : AbstractValidator&lt;ModerateReviewCommand&gt;
{
    public ModerateReviewCommandValidator()
    {
        RuleFor(x =&gt; x.ReviewId)
            .NotEmpty()
            .WithMessage("ReviewId es requerido");

        RuleFor(x =&gt; x.ModeratorId)
            .NotEmpty()
            .WithMessage("ModeratorId es requerido");

        RuleFor(x =&gt; x.RejectionReason)
            .NotEmpty()
            .When(x =&gt; !x.IsApproved)
            .WithMessage("La razón de rechazo es requerida cuando se rechaza una review")
            .MaximumLength(500)
            .WithMessage("La razón de rechazo no puede exceder 500 caracteres");
    }
}