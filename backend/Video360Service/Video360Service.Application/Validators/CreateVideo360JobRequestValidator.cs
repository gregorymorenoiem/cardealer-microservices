using FluentValidation;
using Video360Service.Application.DTOs;

namespace Video360Service.Application.Validators;

/// <summary>
/// Validador para CreateVideo360JobRequest
/// </summary>
public class CreateVideo360JobRequestValidator : AbstractValidator<CreateVideo360JobRequest>
{
    private const int MaxVideoSizeMb = 100; // 100 MB
    private const int MaxBase64Length = MaxVideoSizeMb * 1024 * 1024 * 4 / 3; // Base64 es ~33% más grande
    
    public CreateVideo360JobRequestValidator()
    {
        // Debe tener VideoUrl O VideoBase64
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.VideoUrl) || !string.IsNullOrEmpty(x.VideoBase64))
            .WithMessage("Debe proporcionar VideoUrl o VideoBase64");
        
        // VideoUrl debe ser una URL válida si se proporciona
        When(x => !string.IsNullOrEmpty(x.VideoUrl), () =>
        {
            RuleFor(x => x.VideoUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("VideoUrl debe ser una URL HTTP/HTTPS válida");
        });
        
        // VideoBase64 no debe exceder el tamaño máximo
        When(x => !string.IsNullOrEmpty(x.VideoBase64), () =>
        {
            RuleFor(x => x.VideoBase64)
                .MaximumLength(MaxBase64Length)
                .WithMessage($"El video no debe exceder {MaxVideoSizeMb}MB");
            
            RuleFor(x => x.VideoBase64)
                .Must(BeValidBase64)
                .WithMessage("VideoBase64 no es un string Base64 válido");
        });
        
        // FileName
        RuleFor(x => x.FileName)
            .MaximumLength(255)
            .WithMessage("El nombre del archivo no debe exceder 255 caracteres");
        
        // FrameCount: entre 3 y 12
        RuleFor(x => x.FrameCount)
            .InclusiveBetween(3, 12)
            .WithMessage("FrameCount debe estar entre 3 y 12 (recomendado: 6)");
        
        // Quality: entre 1 y 100
        RuleFor(x => x.Quality)
            .InclusiveBetween(1, 100)
            .WithMessage("Quality debe estar entre 1 y 100");
        
        // Width: si se proporciona, debe ser positivo y razonable
        When(x => x.Width.HasValue, () =>
        {
            RuleFor(x => x.Width!.Value)
                .InclusiveBetween(100, 7680) // Hasta 8K
                .WithMessage("Width debe estar entre 100 y 7680 píxeles");
        });
        
        // Height: si se proporciona, debe ser positivo y razonable
        When(x => x.Height.HasValue, () =>
        {
            RuleFor(x => x.Height!.Value)
                .InclusiveBetween(100, 4320) // Hasta 8K
                .WithMessage("Height debe estar entre 100 y 4320 píxeles");
        });
        
        // SpecificTimestamps: si se proporcionan, deben ser válidos
        When(x => x.SpecificTimestamps != null && x.SpecificTimestamps.Length > 0, () =>
        {
            RuleFor(x => x.SpecificTimestamps)
                .Must(ts => ts!.All(t => t >= 0))
                .WithMessage("Todos los timestamps deben ser >= 0");
            
            RuleFor(x => x.SpecificTimestamps)
                .Must((request, ts) => ts!.Length == request.FrameCount)
                .WithMessage("La cantidad de timestamps debe coincidir con FrameCount");
        });
        
        // CallbackUrl: debe ser URL válida si se proporciona
        When(x => !string.IsNullOrEmpty(x.CallbackUrl), () =>
        {
            RuleFor(x => x.CallbackUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("CallbackUrl debe ser una URL HTTP/HTTPS válida");
        });
        
        // Priority
        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 100)
            .WithMessage("Priority debe estar entre 0 y 100");
    }
    
    private static bool BeValidBase64(string? base64)
    {
        if (string.IsNullOrEmpty(base64)) return true;
        
        try
        {
            // Remover prefijo data:video/... si existe
            var base64Data = base64;
            if (base64Data.Contains(','))
            {
                base64Data = base64Data.Split(',')[1];
            }
            
            var buffer = new Span<byte>(new byte[base64Data.Length]);
            return Convert.TryFromBase64String(base64Data, buffer, out _);
        }
        catch
        {
            return false;
        }
    }
}
