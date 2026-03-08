using FluentValidation;
using AdminService.Application.UseCases.Content;

namespace AdminService.Application.Validators.Content;

/// <summary>
/// Validator for CreateBannerCommand (wraps CreateBannerRequest).
/// Validates all string fields in the request DTO.
/// </summary>
public class CreateBannerCommandValidator : AbstractValidator<CreateBannerCommand>
{
    private static readonly string[] ValidPlacements = { "home-hero", "home-sidebar", "search-top", "search-sidebar", "listing-bottom", "global" };
    private static readonly string[] ValidStatuses = { "active", "inactive", "scheduled", "expired" };

    public CreateBannerCommandValidator()
    {
        RuleFor(x => x.Data).NotNull().WithMessage("Banner data is required.");

        When(x => x.Data != null, () =>
        {
            RuleFor(x => x.Data.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Image)
                .NotEmpty().WithMessage("Image URL is required.")
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Link)
                .NotEmpty().WithMessage("Link is required.")
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Placement)
                .NotEmpty().WithMessage("Placement is required.")
                .Must(p => ValidPlacements.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Placement must be one of: {string.Join(", ", ValidPlacements)}.")
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Subtitle)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.Subtitle));

            RuleFor(x => x.Data.MobileImage)
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.MobileImage));

            RuleFor(x => x.Data.CtaText)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.CtaText));
        });
    }
}

/// <summary>
/// Validator for UpdateBannerCommand (wraps UpdateBannerRequest).
/// Validates BannerId and all string fields in the request DTO.
/// </summary>
public class UpdateBannerCommandValidator : AbstractValidator<UpdateBannerCommand>
{
    private static readonly string[] ValidPlacements = { "home-hero", "home-sidebar", "search-top", "search-sidebar", "listing-bottom", "global" };
    private static readonly string[] ValidStatuses = { "active", "inactive", "scheduled", "expired" };

    public UpdateBannerCommandValidator()
    {
        RuleFor(x => x.BannerId)
            .NotEmpty().WithMessage("Banner ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Data).NotNull().WithMessage("Banner data is required.");

        When(x => x.Data != null, () =>
        {
            RuleFor(x => x.Data.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Image)
                .NotEmpty().WithMessage("Image URL is required.")
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Link)
                .NotEmpty().WithMessage("Link is required.")
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Placement)
                .NotEmpty().WithMessage("Placement is required.")
                .Must(p => ValidPlacements.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Placement must be one of: {string.Join(", ", ValidPlacements)}.")
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .MaximumLength(50)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Data.Subtitle)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.Subtitle));

            RuleFor(x => x.Data.MobileImage)
                .MaximumLength(2048)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.MobileImage));

            RuleFor(x => x.Data.CtaText)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrEmpty(x.Data.CtaText));
        });
    }
}

/// <summary>
/// Validator for DeleteBannerCommand.
/// Validates BannerId with NoSqlInjection/NoXss.
/// </summary>
public class DeleteBannerCommandValidator : AbstractValidator<DeleteBannerCommand>
{
    public DeleteBannerCommandValidator()
    {
        RuleFor(x => x.BannerId)
            .NotEmpty().WithMessage("Banner ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}

/// <summary>
/// Validator for RecordBannerViewCommand.
/// Validates BannerId with NoSqlInjection/NoXss.
/// </summary>
public class RecordBannerViewCommandValidator : AbstractValidator<RecordBannerViewCommand>
{
    public RecordBannerViewCommandValidator()
    {
        RuleFor(x => x.BannerId)
            .NotEmpty().WithMessage("Banner ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}

/// <summary>
/// Validator for RecordBannerClickCommand.
/// Validates BannerId with NoSqlInjection/NoXss.
/// </summary>
public class RecordBannerClickCommandValidator : AbstractValidator<RecordBannerClickCommand>
{
    public RecordBannerClickCommandValidator()
    {
        RuleFor(x => x.BannerId)
            .NotEmpty().WithMessage("Banner ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}

/// <summary>
/// Validator for GetPublicBannersQuery.
/// Validates Placement with NoSqlInjection/NoXss.
/// </summary>
public class GetPublicBannersQueryValidator : AbstractValidator<GetPublicBannersQuery>
{
    public GetPublicBannersQueryValidator()
    {
        RuleFor(x => x.Placement)
            .NotEmpty().WithMessage("Placement is required.")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();
    }
}
