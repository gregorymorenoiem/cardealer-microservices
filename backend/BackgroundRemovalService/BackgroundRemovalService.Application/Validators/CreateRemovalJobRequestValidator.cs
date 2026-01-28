using BackgroundRemovalService.Application.DTOs;
using FluentValidation;

namespace BackgroundRemovalService.Application.Validators;

public class CreateRemovalJobRequestValidator : AbstractValidator<CreateRemovalJobRequest>
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };
    private const int MaxBase64SizeMb = 12;
    
    public CreateRemovalJobRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.ImageUrl) || !string.IsNullOrEmpty(x.ImageBase64))
            .WithMessage("Debe proporcionar ImageUrl o ImageBase64");
        
        When(x => !string.IsNullOrEmpty(x.ImageUrl), () =>
        {
            RuleFor(x => x.ImageUrl)
                .Must(BeAValidUrl!)
                .WithMessage("ImageUrl debe ser una URL válida");
        });
        
        When(x => !string.IsNullOrEmpty(x.ImageBase64), () =>
        {
            RuleFor(x => x.ImageBase64)
                .Must(BeValidBase64!)
                .WithMessage("ImageBase64 debe ser una cadena Base64 válida")
                .Must(NotExceedMaxSize!)
                .WithMessage($"La imagen no debe exceder {MaxBase64SizeMb}MB");
        });
        
        When(x => !string.IsNullOrEmpty(x.FileName), () =>
        {
            RuleFor(x => x.FileName)
                .Must(HaveValidImageExtension!)
                .WithMessage($"El archivo debe tener una extensión válida: {string.Join(", ", AllowedImageExtensions)}");
        });
        
        RuleFor(x => x.Quality)
            .InclusiveBetween(1, 100)
            .WithMessage("Quality debe estar entre 1 y 100");
        
        RuleFor(x => x.CropMargin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CropMargin debe ser mayor o igual a 0");
        
        When(x => x.Width.HasValue, () =>
        {
            RuleFor(x => x.Width!.Value)
                .InclusiveBetween(1, 10000)
                .WithMessage("Width debe estar entre 1 y 10000");
        });
        
        When(x => x.Height.HasValue, () =>
        {
            RuleFor(x => x.Height!.Value)
                .InclusiveBetween(1, 10000)
                .WithMessage("Height debe estar entre 1 y 10000");
        });
        
        When(x => !string.IsNullOrEmpty(x.BackgroundColor), () =>
        {
            RuleFor(x => x.BackgroundColor)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$|^[a-zA-Z]+$")
                .WithMessage("BackgroundColor debe ser un color hexadecimal (#RRGGBB) o nombre de color");
        });
        
        When(x => !string.IsNullOrEmpty(x.CallbackUrl), () =>
        {
            RuleFor(x => x.CallbackUrl)
                .Must(BeAValidUrl!)
                .WithMessage("CallbackUrl debe ser una URL válida");
        });
    }
    
    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    
    private static bool BeValidBase64(string base64)
    {
        try
        {
            // Remover el prefijo data:image/xxx;base64, si existe
            var cleanBase64 = base64;
            if (base64.Contains(','))
            {
                cleanBase64 = base64.Split(',')[1];
            }
            
            Convert.FromBase64String(cleanBase64);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool NotExceedMaxSize(string base64)
    {
        try
        {
            var cleanBase64 = base64;
            if (base64.Contains(','))
            {
                cleanBase64 = base64.Split(',')[1];
            }
            
            var bytes = Convert.FromBase64String(cleanBase64);
            return bytes.Length <= MaxBase64SizeMb * 1024 * 1024;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool HaveValidImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(extension);
    }
}
