namespace KYCService.Application.Services;

/// <summary>
/// Shared helper for masking PII (Personally Identifiable Information) in API responses and logs.
/// Required by Ley 172-13 Art. 31 (data minimization in outputs).
/// </summary>
public static class PiiMaskingHelper
{
    /// <summary>
    /// Masks a document number showing only the last 4 characters.
    /// Example: "40212345678" → "*******5678"
    /// </summary>
    public static string MaskDocumentNumber(string? documentNumber)
    {
        if (string.IsNullOrEmpty(documentNumber) || documentNumber.Length <= 4)
            return "****";
        return new string('*', documentNumber.Length - 4) + documentNumber[^4..];
    }

    /// <summary>
    /// Masks an email address showing only the first 2 characters and the domain.
    /// Example: "juan.perez@gmail.com" → "ju***@gmail.com"
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "****";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "****";

        var prefix = email[..Math.Min(2, atIndex)];
        var domain = email[atIndex..];
        return prefix + "***" + domain;
    }
}
