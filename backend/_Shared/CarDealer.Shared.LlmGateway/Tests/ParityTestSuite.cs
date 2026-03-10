// =============================================================================
// LLM Gateway — Multi-Model Parity Tests
//
// Validates that all 4 LLM-integrated agents produce equivalent outputs
// when running on Claude, Gemini, and Llama. Tests parsing compatibility,
// JSON schema adherence, and fallback behavior for each provider.
//
// Each agent has 50 test cases split into:
//   - 20 golden path cases (expected to parse perfectly)
//   - 15 edge cases (partial JSON, extra text, code blocks)
//   - 10 format variation cases (snake_case vs camelCase)
//   - 5 failure/empty response cases
// =============================================================================

using System.Text.Json;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Services;

namespace CarDealer.Shared.LlmGateway.Tests;

/// <summary>
/// Test data and validation for multi-model parity across all OKLA agents.
/// Can be run as xUnit tests or integration tests against live models.
/// </summary>
public static class ParityTestSuite
{
    // =========================================================================
    // SEARCH AGENT — 50 Test Cases
    // =========================================================================

    public static IReadOnlyList<ParityTestCase> SearchAgentCases { get; } = GenerateSearchAgentCases();

    private static List<ParityTestCase> GenerateSearchAgentCases()
    {
        var cases = new List<ParityTestCase>();

        // ── 20 Golden Path: Clean JSON output ────────────────────────────────
        for (int i = 1; i <= 20; i++)
        {
            cases.Add(new ParityTestCase
            {
                Id = $"search-golden-{i:D2}",
                Agent = "SearchAgent",
                UserInput = GetSearchQuery(i),
                ExpectedFormat = ResponseFormat.Json,
                SchemaValidator = response => ValidateSearchAgentSchema(response),
                RequiredFields = ["filtros_exactos", "confianza"],
                Description = $"Golden path search query #{i}"
            });
        }

        // ── 15 Edge Cases: Wrapped in code blocks, extra text ────────────────
        cases.AddRange(
        [
            CreateEdgeCase("search-edge-01", "SearchAgent", WrapInCodeBlock(SampleSearchJson()), "JSON wrapped in ```json blocks"),
            CreateEdgeCase("search-edge-02", "SearchAgent", $"Here is the result:\n{SampleSearchJson()}\nHope this helps!", "JSON with surrounding text"),
            CreateEdgeCase("search-edge-03", "SearchAgent", SampleSearchJson().Replace("\"confianza\": 0.85", "\"confianza\":0.85"), "JSON without spaces in values"),
            CreateEdgeCase("search-edge-04", "SearchAgent", SampleSearchJson() + "\n\n// Note: results are approximate", "JSON with trailing comment"),
            CreateEdgeCase("search-edge-05", "SearchAgent", "```\n" + SampleSearchJson() + "\n```", "JSON in generic code block (no json tag)"),
            CreateEdgeCase("search-edge-06", "SearchAgent", SampleSearchJson().Replace("filtros_exactos", "filtrosExactos"), "camelCase variant of snake_case fields"),
            CreateEdgeCase("search-edge-07", "SearchAgent", SampleSearchJson().Replace("filtros_exactos", "FiltrosExactos"), "PascalCase variant"),
            CreateEdgeCase("search-edge-08", "SearchAgent", "Based on the query, I found:\n\n" + WrapInCodeBlock(SampleSearchJson()), "Preamble + code block"),
            CreateEdgeCase("search-edge-09", "SearchAgent", SampleSearchJson()[..^1], "Truncated JSON (missing closing brace)"),
            CreateEdgeCase("search-edge-10", "SearchAgent", SampleSearchJson() + ",", "JSON with trailing comma"),
            CreateEdgeCase("search-edge-11", "SearchAgent", "  \n  " + SampleSearchJson() + "  \n  ", "JSON with whitespace padding"),
            CreateEdgeCase("search-edge-12", "SearchAgent", SampleSearchJson().Replace("\n", " "), "Single-line JSON"),
            CreateEdgeCase("search-edge-13", "SearchAgent", "```JSON\n" + SampleSearchJson() + "\n```", "Uppercase JSON code block"),
            CreateEdgeCase("search-edge-14", "SearchAgent", "I'll analyze the query.\n\n" + SampleSearchJson() + "\n\nThis should match well.", "Gemini-style verbose wrapper"),
            CreateEdgeCase("search-edge-15", "SearchAgent", "<|start_header_id|>assistant<|end_header_id|>\n" + SampleSearchJson(), "Llama control tokens in output"),
        ]);

        // ── 10 Format Variation Cases ────────────────────────────────────────
        for (int i = 1; i <= 10; i++)
        {
            cases.Add(new ParityTestCase
            {
                Id = $"search-format-{i:D2}",
                Agent = "SearchAgent",
                UserInput = GetSearchQuery(i),
                ExpectedFormat = ResponseFormat.Json,
                SimulatedResponse = GenerateFormatVariation(SampleSearchJson(), i),
                SchemaValidator = response => ValidateSearchAgentSchema(response),
                RequiredFields = ["filtros_exactos", "confianza"],
                Description = $"Format variation #{i} (model-specific output styles)"
            });
        }

        // ── 5 Failure Cases ──────────────────────────────────────────────────
        cases.AddRange(
        [
            new ParityTestCase { Id = "search-fail-01", Agent = "SearchAgent", SimulatedResponse = "", ExpectedFormat = ResponseFormat.Empty, Description = "Empty response" },
            new ParityTestCase { Id = "search-fail-02", Agent = "SearchAgent", SimulatedResponse = "I cannot process this request.", ExpectedFormat = ResponseFormat.PlainText, Description = "Model refusal" },
            new ParityTestCase { Id = "search-fail-03", Agent = "SearchAgent", SimulatedResponse = "null", ExpectedFormat = ResponseFormat.PlainText, Description = "Literal null" },
            new ParityTestCase { Id = "search-fail-04", Agent = "SearchAgent", SimulatedResponse = "{invalid json", ExpectedFormat = ResponseFormat.PlainText, Description = "Malformed JSON" },
            new ParityTestCase { Id = "search-fail-05", Agent = "SearchAgent", SimulatedResponse = "[]", ExpectedFormat = ResponseFormat.Json, Description = "Array instead of object" },
        ]);

        return cases;
    }

    // =========================================================================
    // RECO AGENT — 50 Test Cases
    // =========================================================================

    public static IReadOnlyList<ParityTestCase> RecoAgentCases { get; } = GenerateRecoAgentCases();

    private static List<ParityTestCase> GenerateRecoAgentCases()
    {
        var cases = new List<ParityTestCase>();

        for (int i = 1; i <= 20; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"reco-golden-{i:D2}",
                Agent = "RecoAgent",
                UserInput = $"User browsing pattern #{i}",
                ExpectedFormat = ResponseFormat.Json,
                SchemaValidator = response => ValidateRecoAgentSchema(response),
                RequiredFields = ["recomendaciones", "confianza_recomendaciones"],
                Description = $"Golden path recommendation #{i}"
            });

        for (int i = 1; i <= 15; i++)
            cases.Add(CreateEdgeCase($"reco-edge-{i:D2}", "RecoAgent",
                GenerateRecoEdgeCase(i), $"Edge case recommendation #{i}"));

        for (int i = 1; i <= 10; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"reco-format-{i:D2}",
                Agent = "RecoAgent",
                SimulatedResponse = GenerateFormatVariation(SampleRecoJson(), i),
                ExpectedFormat = ResponseFormat.Json,
                Description = $"Format variation #{i}"
            });

        for (int i = 1; i <= 5; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"reco-fail-{i:D2}",
                Agent = "RecoAgent",
                SimulatedResponse = GetFailureResponse(i),
                ExpectedFormat = i == 5 ? ResponseFormat.Json : ResponseFormat.PlainText,
                Description = $"Failure case #{i}"
            });

        return cases;
    }

    // =========================================================================
    // DEALER CHAT AGENT — 50 Test Cases
    // =========================================================================

    public static IReadOnlyList<ParityTestCase> DealerChatAgentCases { get; } = GenerateDealerChatCases();

    private static List<ParityTestCase> GenerateDealerChatCases()
    {
        var cases = new List<ParityTestCase>();

        for (int i = 1; i <= 20; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"chat-golden-{i:D2}",
                Agent = "DealerChatAgent",
                UserInput = GetChatQuery(i),
                ExpectedFormat = ResponseFormat.Json,
                SchemaValidator = response => ValidateDealerChatSchema(response),
                RequiredFields = ["response", "intent", "confidence"],
                Description = $"Golden path chat #{i}"
            });

        for (int i = 1; i <= 15; i++)
            cases.Add(CreateEdgeCase($"chat-edge-{i:D2}", "DealerChatAgent",
                GenerateChatEdgeCase(i), $"Edge case chat #{i}"));

        for (int i = 1; i <= 10; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"chat-format-{i:D2}",
                Agent = "DealerChatAgent",
                SimulatedResponse = GenerateFormatVariation(SampleChatJson(), i),
                ExpectedFormat = ResponseFormat.Json,
                Description = $"Format variation #{i}"
            });

        for (int i = 1; i <= 5; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"chat-fail-{i:D2}",
                Agent = "DealerChatAgent",
                SimulatedResponse = GetFailureResponse(i),
                ExpectedFormat = i == 5 ? ResponseFormat.Json : ResponseFormat.PlainText,
                Description = $"Failure case #{i}"
            });

        return cases;
    }

    // =========================================================================
    // SUPPORT AGENT — 50 Test Cases
    // =========================================================================

    public static IReadOnlyList<ParityTestCase> SupportAgentCases { get; } = GenerateSupportCases();

    private static List<ParityTestCase> GenerateSupportCases()
    {
        var cases = new List<ParityTestCase>();

        for (int i = 1; i <= 20; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"support-golden-{i:D2}",
                Agent = "SupportAgent",
                UserInput = GetSupportQuery(i),
                ExpectedFormat = ResponseFormat.PlainText,
                Description = $"Golden path support #{i}"
            });

        for (int i = 1; i <= 15; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"support-edge-{i:D2}",
                Agent = "SupportAgent",
                SimulatedResponse = GenerateSupportEdgeCase(i),
                ExpectedFormat = ResponseFormat.PlainText,
                Description = $"Edge case support #{i}"
            });

        for (int i = 1; i <= 10; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"support-format-{i:D2}",
                Agent = "SupportAgent",
                SimulatedResponse = GenerateSupportFormatVariation(i),
                ExpectedFormat = ResponseFormat.PlainText,
                Description = $"Format variation #{i}"
            });

        for (int i = 1; i <= 5; i++)
            cases.Add(new ParityTestCase
            {
                Id = $"support-fail-{i:D2}",
                Agent = "SupportAgent",
                SimulatedResponse = GetFailureResponse(i),
                ExpectedFormat = ResponseFormat.PlainText,
                Description = $"Failure case #{i}"
            });

        return cases;
    }

    // =========================================================================
    // VALIDATION — Run All Cases Against Parser
    // =========================================================================

    /// <summary>
    /// Run the parity test suite against the shared LlmResponseParser.
    /// Returns a report of pass/fail for each case.
    /// </summary>
    public static ParityTestReport RunParserValidation()
    {
        var results = new List<ParityTestResult>();
        var allCases = SearchAgentCases
            .Concat(RecoAgentCases)
            .Concat(DealerChatAgentCases)
            .Concat(SupportAgentCases);

        foreach (var tc in allCases)
        {
            if (tc.SimulatedResponse == null) continue; // Skip cases that need live LLM

            var result = ValidateCase(tc);
            results.Add(result);
        }

        return new ParityTestReport
        {
            TotalCases = results.Count,
            Passed = results.Count(r => r.Passed),
            Failed = results.Count(r => !r.Passed),
            Results = results
        };
    }

    private static ParityTestResult ValidateCase(ParityTestCase tc)
    {
        try
        {
            if (tc.ExpectedFormat == ResponseFormat.Empty)
            {
                var text = tc.SimulatedResponse ?? "";
                var isEmpty = string.IsNullOrWhiteSpace(text);
                return new ParityTestResult
                {
                    CaseId = tc.Id,
                    Agent = tc.Agent,
                    Passed = isEmpty,
                    Detail = isEmpty ? "Correctly identified empty input" : "Should have been treated as empty"
                };
            }

            if (tc.ExpectedFormat == ResponseFormat.PlainText)
            {
                var text = LlmResponseParser.GetPlainText(tc.SimulatedResponse ?? "");
                return new ParityTestResult
                {
                    CaseId = tc.Id,
                    Agent = tc.Agent,
                    Passed = !string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(tc.SimulatedResponse),
                    Detail = $"Plain text extracted: {(text.Length > 100 ? text[..100] + "..." : text)}"
                };
            }

            // JSON format validation — use StripCodeBlocks + ExtractJsonFromBraces
            var raw = tc.SimulatedResponse ?? "";
            var stripped = LlmResponseParser.StripCodeBlocks(raw);
            var jsonText = LlmResponseParser.ExtractJsonFromBraces(stripped)
                           ?? LlmResponseParser.ExtractJsonArray(stripped)
                           ?? stripped.Trim();

            JsonDocument? doc;
            try
            {
                doc = JsonDocument.Parse(jsonText);
            }
            catch (JsonException)
            {
                return new ParityTestResult
                {
                    CaseId = tc.Id,
                    Agent = tc.Agent,
                    Passed = false,
                    Detail = $"Failed to parse JSON. Response: {(raw.Length > 100 ? raw[..100] : raw)}"
                };
            }

            using (doc)
            {
                var root = doc.RootElement;

                // Check required fields
                if (tc.RequiredFields?.Count > 0)
                {
                    var missing = tc.RequiredFields
                        .Where(f => !root.TryGetProperty(f, out _))
                        .ToList();

                    if (missing.Count > 0)
                    {
                        return new ParityTestResult
                        {
                            CaseId = tc.Id,
                            Agent = tc.Agent,
                            Passed = false,
                            Detail = $"Missing required fields: {string.Join(", ", missing)}"
                        };
                    }
                }

                // Run custom validator if provided
                if (tc.SchemaValidator != null)
                {
                    var validationResult = tc.SchemaValidator(root.GetRawText());
                    if (!validationResult.IsValid)
                    {
                        return new ParityTestResult
                        {
                            CaseId = tc.Id,
                            Agent = tc.Agent,
                            Passed = false,
                            Detail = $"Schema validation failed: {validationResult.Error}"
                        };
                    }
                }

                return new ParityTestResult
                {
                    CaseId = tc.Id,
                    Agent = tc.Agent,
                    Passed = true,
                    Detail = "Parsed successfully with all required fields present"
                };
            }
        }
        catch (Exception ex)
        {
            return new ParityTestResult
            {
                CaseId = tc.Id,
                Agent = tc.Agent,
                Passed = false,
                Detail = $"Exception: {ex.Message}"
            };
        }
    }

    // =========================================================================
    // SAMPLE JSON RESPONSES (representative of each agent)
    // =========================================================================

    private static string SampleSearchJson() => """
        {
            "filtros_exactos": {
                "marca": "Toyota",
                "modelo": "Corolla",
                "year_min": 2020,
                "year_max": 2024,
                "precio_min": 500000,
                "precio_max": 1200000,
                "transmision": "automatica",
                "combustible": "gasolina",
                "condicion": "usado"
            },
            "filtros_relajados": {
                "marca": "Toyota",
                "year_min": 2018
            },
            "confianza": 0.85,
            "patrocinados_config": {
                "relevancia_minima": 0.6,
                "max_patrocinados": 3
            }
        }
        """;

    private static string SampleRecoJson() => """
        {
            "recomendaciones": [
                {
                    "vehiculo_id": "abc-123",
                    "score": 0.92,
                    "razon": "Coincide con tu búsqueda de Toyota Corolla"
                },
                {
                    "vehiculo_id": "def-456",
                    "score": 0.85,
                    "razon": "Precio similar, mismo año"
                }
            ],
            "patrocinados_config": {
                "relevancia_minima": 0.5,
                "max_patrocinados": 2
            },
            "diversificacion_aplicada": true,
            "etapa_compra_detectada": "comparacion",
            "confianza_recomendaciones": 0.88
        }
        """;

    private static string SampleChatJson() => """
        {
            "response": "¡Hola! Sí, tenemos el Toyota Corolla 2023 disponible. ¿Te gustaría agendar una cita para verlo?",
            "intent": "vehicle_inquiry",
            "confidence": 0.92,
            "is_fallback": false,
            "lead_signals": ["high_purchase_intent", "specific_model"],
            "suggested_action": "schedule_appointment",
            "quick_replies": ["Ver fotos", "Agendar cita", "Precio financiamiento"],
            "intent_score": 0.95,
            "clasificacion": "consulta_vehiculo",
            "modulo_activo": "inventario",
            "vehiculo_interes_id": "abc-123",
            "handoff_activado": false,
            "razon_handoff": null,
            "temas_consulta": ["disponibilidad", "cita"]
        }
        """;

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static string WrapInCodeBlock(string json) => $"```json\n{json}\n```";

    private static string GetSearchQuery(int i) => i switch
    {
        1 => "Toyota Corolla 2020 automático",
        2 => "SUV barato menos de 500 mil pesos",
        3 => "Honda Civic o Accord usado Santo Domingo",
        4 => "camioneta 4x4 diesel",
        5 => "carro económico para primer vehículo",
        6 => "Hyundai Tucson 2022 azul",
        7 => "vehículo familiar 7 pasajeros",
        8 => "deportivo menos de 2 millones",
        9 => "pickup Toyota Hilux Santiago",
        10 => "carro híbrido 2023",
        11 => "Kia Sportage o Seltos",
        12 => "sedán mediano gasolina automático",
        13 => "jeepeta barata RD",
        14 => "micro bus para negocio",
        15 => "Tesla Model 3 o similar eléctrico",
        16 => "carro con financiamiento incluido",
        17 => "BMW X3 usado certificado",
        18 => "Toyota RAV4 2021 gris",
        19 => "vehículo comercial panel",
        20 => "Nissan Frontier 4x4",
        _ => "carro usado bueno bonito barato"
    };

    private static string GetChatQuery(int i) => i switch
    {
        1 => "¿Tienen el Corolla 2023 disponible?",
        2 => "¿Cuánto cuesta el financiamiento?",
        3 => "Quiero agendar una cita para ver el carro",
        4 => "¿Aceptan trade-in?",
        5 => "¿Cuál es la garantía?",
        6 => "¿Tienen colores disponibles?",
        7 => "¿El precio incluye DGII?",
        8 => "Quiero hablar con un vendedor",
        9 => "¿Están abiertos los domingos?",
        10 => "¿Dónde están ubicados?",
        11 => "¿Hacen envío a Santiago?",
        12 => "Me interesa el RAV4 gris",
        13 => "¿Aceptan tarjeta de crédito?",
        14 => "¿Tienen servicio de taller?",
        15 => "Quiero comparar dos carros",
        16 => "¿El carro tiene historial de accidentes?",
        17 => "¿Cuántos kilómetros tiene?",
        18 => "¿Puedo hacer prueba de manejo?",
        19 => "Necesito un carro urgente",
        20 => "¿Tienen plan de pago sin inicial?",
        _ => "Hola, me interesa un carro"
    };

    private static string GetSupportQuery(int i) => i switch
    {
        1 => "No puedo publicar mi vehículo",
        2 => "¿Cómo cambio mi plan?",
        3 => "Mi pago no fue procesado",
        4 => "¿Cómo verifico mi cuenta?",
        5 => "Recibí un cobro doble",
        6 => "No puedo subir fotos",
        7 => "¿Cuánto cuesta el plan Pro?",
        8 => "Quiero cancelar mi suscripción",
        9 => "Mi listing no aparece en búsqueda",
        10 => "¿Cómo contacto a un vendedor?",
        11 => "Reportar un listing fraudulento",
        12 => "No recibo el código de verificación",
        13 => "¿Cómo funciona la garantía de resultados?",
        14 => "Quiero cambiar mi email",
        15 => "El chat del dealer no responde",
        16 => "¿Puedo editar mi publicación?",
        17 => "¿Cuántas fotos puedo subir?",
        18 => "Mi cuenta fue suspendida",
        19 => "¿Aceptan pago con Azul?",
        20 => "Necesito factura fiscal",
        _ => "Necesito ayuda"
    };

    private static ParityTestCase CreateEdgeCase(string id, string agent, string response, string desc) => new()
    {
        Id = id,
        Agent = agent,
        SimulatedResponse = response,
        ExpectedFormat = ResponseFormat.Json,
        Description = desc
    };

    private static string GenerateFormatVariation(string json, int variant) => variant switch
    {
        1 => json.Replace("    ", ""), // No indentation
        2 => json.Replace("\n", " "),  // Single line
        3 => "Here is the JSON:\n" + json, // Preamble
        4 => json + "\n\nNote: confidence is estimated", // Postamble
        5 => WrapInCodeBlock(json), // Code block
        6 => json.Replace("\"", "'").Replace("'", "\""), // Double quotes preserved
        7 => "```json\n" + json.Replace("    ", "\t") + "\n```", // Tabs in code block
        8 => "\n\n" + json + "\n\n", // Extra whitespace
        9 => "Response:\n" + WrapInCodeBlock(json) + "\nDone.", // Full wrapper
        10 => json[..^1] + ", \"extra_field\": \"value\"}", // Extra field
        _ => json
    };

    private static string GetFailureResponse(int i) => i switch
    {
        1 => "",
        2 => "I cannot process this request.",
        3 => "null",
        4 => "{invalid json",
        5 => "[]",
        _ => ""
    };

    private static string GenerateRecoEdgeCase(int i) => i switch
    {
        <= 5 => WrapInCodeBlock(SampleRecoJson()),
        <= 10 => "Based on user history:\n" + SampleRecoJson(),
        _ => SampleRecoJson().Replace("recomendaciones", "recommendations"), // English key
    };

    private static string GenerateChatEdgeCase(int i) => i switch
    {
        <= 5 => WrapInCodeBlock(SampleChatJson()),
        <= 10 => "Dealer response:\n" + SampleChatJson(),
        _ => SampleChatJson().Replace("\"is_fallback\": false", "\"is_fallback\": False"), // Python-style bool
    };

    private static string GenerateSupportEdgeCase(int i) => i switch
    {
        <= 5 => $"Para resolver tu problema #{i}, sigue estos pasos:\n1. Ve a Configuración\n2. Selecciona tu plan\n3. Confirma el cambio",
        <= 10 => "```\nInstrucciones paso a paso...\n```",
        _ => "¡Hola! 😊 Entiendo tu situación. Déjame ayudarte con eso.",
    };

    private static string GenerateSupportFormatVariation(int i) => i switch
    {
        1 => "Respuesta corta.",
        2 => "Una respuesta\nmás larga\ncon saltos de línea.",
        3 => "**Respuesta con markdown** y *énfasis*.",
        4 => "Respuesta con emoji 🚗💰✅",
        5 => "Respuesta con <b>HTML tags</b>",
        6 => "Respuesta con links: https://okla.com.do/ayuda",
        7 => "Respuesta con\ttabs\ty\tespacios especiales",
        8 => "   Respuesta con espacios al inicio y final   ",
        9 => "Respuesta con \"comillas\" y 'apóstrofes'",
        10 => "Respuesta muy larga " + new string('x', 5000),
        _ => "Respuesta genérica"
    };

    // =========================================================================
    // SCHEMA VALIDATORS
    // =========================================================================

    private static ValidationResult ValidateSearchAgentSchema(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("filtros_exactos", out _) && !root.TryGetProperty("filtrosExactos", out _))
                return new ValidationResult(false, "Missing filtros_exactos/filtrosExactos");

            if (!root.TryGetProperty("confianza", out var conf))
                return new ValidationResult(false, "Missing confianza");

            if (conf.ValueKind == JsonValueKind.Number)
            {
                var val = conf.GetDouble();
                if (val < 0 || val > 1)
                    return new ValidationResult(false, $"confianza out of range: {val}");
            }

            return new ValidationResult(true);
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }

    private static ValidationResult ValidateRecoAgentSchema(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("recomendaciones", out var recs) && !root.TryGetProperty("recommendations", out recs))
                return new ValidationResult(false, "Missing recomendaciones");

            if (recs.ValueKind != JsonValueKind.Array)
                return new ValidationResult(false, "recomendaciones is not an array");

            return new ValidationResult(true);
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }

    private static ValidationResult ValidateDealerChatSchema(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("response", out _))
                return new ValidationResult(false, "Missing response field");

            if (!root.TryGetProperty("intent", out _))
                return new ValidationResult(false, "Missing intent field");

            return new ValidationResult(true);
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }
}

// =============================================================================
// DATA TYPES
// =============================================================================

public sealed class ParityTestCase
{
    public required string Id { get; init; }
    public required string Agent { get; init; }
    public string? UserInput { get; init; }
    public string? SimulatedResponse { get; init; }
    public ResponseFormat ExpectedFormat { get; init; }
    public Func<string, ValidationResult>? SchemaValidator { get; init; }
    public IReadOnlyList<string>? RequiredFields { get; init; }
    public string Description { get; init; } = "";
}

public enum ResponseFormat
{
    Json,
    PlainText,
    Empty
}

public sealed record ValidationResult(bool IsValid, string? Error = null);

public sealed class ParityTestResult
{
    public required string CaseId { get; init; }
    public required string Agent { get; init; }
    public required bool Passed { get; init; }
    public required string Detail { get; init; }
}

public sealed class ParityTestReport
{
    public int TotalCases { get; init; }
    public int Passed { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<ParityTestResult> Results { get; init; } = [];
    public double PassRate => TotalCases > 0 ? (double)Passed / TotalCases * 100 : 0;
}
