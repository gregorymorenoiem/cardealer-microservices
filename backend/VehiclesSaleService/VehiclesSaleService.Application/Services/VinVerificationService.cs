namespace VehiclesSaleService.Application.Services;

/// <summary>
/// Cross-references VIN-decoded data (from NHTSA vPIC) against dealer-declared
/// vehicle attributes. Returns a list of discrepancies that flag potential
/// data entry errors or fraud.
///
/// Used at publish time to gate listing approval:
///   - If discrepancies found → listing goes to PendingReview with explanatory message
///   - Fraud score increases by +20 per critical mismatch (make/model/year)
///   - Body type mismatch is a warning (+10 points) since NHTSA naming varies
///
/// The service is stateless and does NOT call NHTSA itself — it receives the
/// already-decoded result and performs string comparison with fuzzy matching.
/// </summary>
public static class VinVerificationService
{
    /// <summary>
    /// Compare NHTSA-decoded VIN data against dealer-declared vehicle attributes.
    /// </summary>
    /// <param name="nhtsaMake">Make from NHTSA vPIC (e.g., "HONDA")</param>
    /// <param name="nhtsaModel">Model from NHTSA vPIC (e.g., "Civic")</param>
    /// <param name="nhtsaYear">Model year from NHTSA vPIC</param>
    /// <param name="nhtsaBodyClass">Body class from NHTSA vPIC (e.g., "Sedan", "SUV")</param>
    /// <param name="declaredMake">Make declared by dealer</param>
    /// <param name="declaredModel">Model declared by dealer</param>
    /// <param name="declaredYear">Year declared by dealer</param>
    /// <param name="declaredBodyType">Body type declared by dealer (optional)</param>
    /// <returns>Verification result with discrepancies and fraud score impact</returns>
    public static VinVerificationResult Verify(
        string? nhtsaMake,
        string? nhtsaModel,
        int nhtsaYear,
        string? nhtsaBodyClass,
        string declaredMake,
        string declaredModel,
        int declaredYear,
        string? declaredBodyType = null)
    {
        var discrepancies = new List<VinDiscrepancy>();
        int fraudPoints = 0;

        // ── Make comparison (critical) ──────────────────────────
        if (!string.IsNullOrWhiteSpace(nhtsaMake) && !string.IsNullOrWhiteSpace(declaredMake))
        {
            if (!FuzzyMatch(nhtsaMake, declaredMake))
            {
                fraudPoints += 20;
                discrepancies.Add(new VinDiscrepancy(
                    Field: "Marca",
                    DeclaredValue: declaredMake,
                    VinValue: nhtsaMake,
                    Severity: DiscrepancySeverity.Critical,
                    Points: 20,
                    Message: $"El VIN indica marca \"{nhtsaMake}\" pero declaraste \"{declaredMake}\"."
                ));
            }
        }

        // ── Model comparison (critical) ─────────────────────────
        if (!string.IsNullOrWhiteSpace(nhtsaModel) && !string.IsNullOrWhiteSpace(declaredModel))
        {
            if (!FuzzyMatch(nhtsaModel, declaredModel))
            {
                fraudPoints += 20;
                discrepancies.Add(new VinDiscrepancy(
                    Field: "Modelo",
                    DeclaredValue: declaredModel,
                    VinValue: nhtsaModel,
                    Severity: DiscrepancySeverity.Critical,
                    Points: 20,
                    Message: $"El VIN indica modelo \"{nhtsaModel}\" pero declaraste \"{declaredModel}\"."
                ));
            }
        }

        // ── Year comparison (critical) ──────────────────────────
        if (nhtsaYear > 0 && declaredYear > 0)
        {
            // Allow ±1 year tolerance (NHTSA model year vs calendar year edge cases)
            if (Math.Abs(nhtsaYear - declaredYear) > 1)
            {
                fraudPoints += 20;
                discrepancies.Add(new VinDiscrepancy(
                    Field: "Año",
                    DeclaredValue: declaredYear.ToString(),
                    VinValue: nhtsaYear.ToString(),
                    Severity: DiscrepancySeverity.Critical,
                    Points: 20,
                    Message: $"El VIN indica año {nhtsaYear} pero declaraste {declaredYear}."
                ));
            }
        }

        // ── Body type comparison (warning — NHTSA naming varies) ─
        if (!string.IsNullOrWhiteSpace(nhtsaBodyClass) && !string.IsNullOrWhiteSpace(declaredBodyType))
        {
            if (!FuzzyMatchBodyType(nhtsaBodyClass, declaredBodyType))
            {
                fraudPoints += 10;
                discrepancies.Add(new VinDiscrepancy(
                    Field: "Tipo de carrocería",
                    DeclaredValue: declaredBodyType,
                    VinValue: nhtsaBodyClass,
                    Severity: DiscrepancySeverity.Warning,
                    Points: 10,
                    Message: $"El VIN indica tipo \"{nhtsaBodyClass}\" pero declaraste \"{declaredBodyType}\". Esto puede ser por diferencias en nomenclatura."
                ));
            }
        }

        // Build dealer-facing explanation
        string? dealerMessage = null;
        if (discrepancies.Count > 0)
        {
            var nhtsaLabel = $"{nhtsaYear} {nhtsaMake ?? "?"} {nhtsaModel ?? "?"}";
            var declaredLabel = $"{declaredYear} {declaredMake} {declaredModel}";
            var details = string.Join(" ", discrepancies.Select(d => d.Message));

            dealerMessage = $"El VIN indica que el vehículo es un {nhtsaLabel.Trim()}, "
                         + $"pero declaraste un {declaredLabel.Trim()}. "
                         + details + " "
                         + "Tu listado será revisado manualmente por nuestro equipo antes de publicarse.";
        }

        return new VinVerificationResult(
            HasDiscrepancies: discrepancies.Count > 0,
            Discrepancies: discrepancies,
            FraudPointsAdded: Math.Min(fraudPoints, 60), // Cap VIN-related fraud at 60 pts
            DealerMessage: dealerMessage
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // Fuzzy matching utilities
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Case-insensitive comparison with common alias handling.
    /// E.g., "HONDA" matches "Honda", "CHEVROLET" matches "Chevy", etc.
    /// </summary>
    private static bool FuzzyMatch(string a, string b)
    {
        // Normalize both
        var na = Normalize(a);
        var nb = Normalize(b);

        // Exact match after normalization
        if (string.Equals(na, nb, StringComparison.OrdinalIgnoreCase))
            return true;

        // One contains the other (handles "HONDA MOTOR CO" vs "Honda")
        if (na.Contains(nb, StringComparison.OrdinalIgnoreCase) ||
            nb.Contains(na, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check known aliases
        return AreKnownAliases(na, nb);
    }

    /// <summary>
    /// Body type matching is more lenient due to NHTSA's verbose naming.
    /// E.g., "Sedan/Saloon" should match "Sedan", "Sport Utility Vehicle" should match "SUV".
    /// </summary>
    private static bool FuzzyMatchBodyType(string nhtsaBody, string declaredBody)
    {
        var na = Normalize(nhtsaBody);
        var nb = Normalize(declaredBody);

        if (string.Equals(na, nb, StringComparison.OrdinalIgnoreCase))
            return true;

        if (na.Contains(nb, StringComparison.OrdinalIgnoreCase) ||
            nb.Contains(na, StringComparison.OrdinalIgnoreCase))
            return true;

        // Known body type aliases
        var bodyAliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["sedan"] = new[] { "saloon", "sedan/saloon", "sedan 4-door" },
            ["suv"] = new[] { "sport utility vehicle", "sport utility", "multipurpose passenger vehicle" },
            ["pickup"] = new[] { "truck", "pickup truck", "standard pickup" },
            ["hatchback"] = new[] { "hatchback/liftback", "hatchback 4-door", "hatchback 5-door" },
            ["coupe"] = new[] { "coupe/sport", "coupe 2-door" },
            ["van"] = new[] { "minivan", "van/minivan", "passenger van" },
            ["wagon"] = new[] { "station wagon", "wagon/sport wagon" },
            ["convertible"] = new[] { "convertible/cabriolet", "roadster" },
            ["crossover"] = new[] { "sport utility vehicle", "crossover utility vehicle", "cuv" },
        };

        foreach (var (key, aliases) in bodyAliases)
        {
            var allTerms = aliases.Append(key).ToArray();
            var aMatch = allTerms.Any(t => na.Contains(t, StringComparison.OrdinalIgnoreCase));
            var bMatch = allTerms.Any(t => nb.Contains(t, StringComparison.OrdinalIgnoreCase));
            if (aMatch && bMatch) return true;
        }

        return false;
    }

    private static string Normalize(string s)
    {
        return s.Trim()
                .Replace("-", " ")
                .Replace("_", " ")
                .Replace("/", " ");
    }

    /// <summary>
    /// Common make aliases in the DR market.
    /// </summary>
    private static bool AreKnownAliases(string a, string b)
    {
        var aliases = new[]
        {
            new[] { "chevrolet", "chevy" },
            new[] { "mercedes", "mercedes benz", "mercedes-benz" },
            new[] { "volkswagen", "vw" },
            new[] { "bmw", "bayerische motoren werke" },
            new[] { "land rover", "landrover" },
            new[] { "mitsubishi", "mitsubishi motors" },
        };

        foreach (var group in aliases)
        {
            var aMatch = group.Any(alias => a.Contains(alias, StringComparison.OrdinalIgnoreCase));
            var bMatch = group.Any(alias => b.Contains(alias, StringComparison.OrdinalIgnoreCase));
            if (aMatch && bMatch) return true;
        }

        return false;
    }
}

// ═══════════════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════════════

public record VinVerificationResult(
    bool HasDiscrepancies,
    List<VinDiscrepancy> Discrepancies,
    int FraudPointsAdded,
    string? DealerMessage
);

public record VinDiscrepancy(
    string Field,
    string DeclaredValue,
    string VinValue,
    DiscrepancySeverity Severity,
    int Points,
    string Message
);

public enum DiscrepancySeverity
{
    Warning,
    Critical
}
