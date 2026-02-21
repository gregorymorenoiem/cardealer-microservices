using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Controller para gestionar métodos de pago guardados del usuario
/// Permite agregar, listar, eliminar y establecer tarjetas como predeterminadas
/// </summary>
[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly ISavedPaymentMethodRepository _repository;
    private readonly IPaymentGatewayRegistry _gatewayRegistry;
    private readonly IGatewayAvailabilityService _availability;
    private readonly ITokenizationService _tokenizationService;
    private readonly ILogger<PaymentMethodsController> _logger;

    // Límite máximo de métodos de pago por usuario
    private const int MaxPaymentMethodsPerUser = 10;

    public PaymentMethodsController(
        ISavedPaymentMethodRepository repository,
        IPaymentGatewayRegistry gatewayRegistry,
        IGatewayAvailabilityService availability,
        ITokenizationService tokenizationService,
        ILogger<PaymentMethodsController> logger)
    {
        _repository = repository;
        _gatewayRegistry = gatewayRegistry;
        _availability = availability;
        _tokenizationService = tokenizationService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos los métodos de pago del usuario autenticado
    /// </summary>
    /// <response code="200">Lista de métodos de pago</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaymentMethodsListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var methods = await _repository.GetActiveByUserIdAsync(userId, cancellationToken);
        var result = methods.ToListDto();

        _logger.LogInformation("User {UserId} retrieved {Count} payment methods", userId, result.Total);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un método de pago específico
    /// </summary>
    /// <param name="id">ID del método de pago</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var method = await _repository.GetByIdAndUserAsync(id, userId, cancellationToken);
        if (method == null)
        {
            return NotFound(new { message = "Método de pago no encontrado" });
        }

        return Ok(method.ToDto());
    }

    /// <summary>
    /// Agrega un nuevo método de pago
    /// Tokeniza la tarjeta usando la pasarela seleccionada
    /// </summary>
    /// <param name="request">Datos del método de pago</param>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] AddPaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Validar límite de métodos de pago
        var currentCount = await _repository.CountByUserIdAsync(userId, cancellationToken);
        if (currentCount >= MaxPaymentMethodsPerUser)
        {
            return BadRequest(new 
            { 
                message = $"Has alcanzado el límite máximo de {MaxPaymentMethodsPerUser} métodos de pago",
                code = "MAX_PAYMENT_METHODS_REACHED"
            });
        }

        // Validar datos requeridos
        if (request.Type == "card")
        {
            if (request.Card == null && string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { message = "Se requieren datos de la tarjeta o un token pre-generado" });
            }
        }

        try
        {
            // Determinar gateway a usar
            var gatewayEnum = DetermineGateway(request.Gateway);

            // Block adding payment methods with disabled gateways (new users)
            var isGatewayEnabled = await _availability.IsEnabledForNewUsersAsync(gatewayEnum, cancellationToken);
            if (!isGatewayEnabled)
            {
                _logger.LogWarning("User {UserId} tried to add payment method with disabled gateway {Gateway}", userId, gatewayEnum);
                return BadRequest(new
                {
                    message = $"La pasarela de pago {gatewayEnum} no está disponible actualmente. Por favor selecciona otra.",
                    code = "GATEWAY_DISABLED"
                });
            }

            var provider = _gatewayRegistry.Get(gatewayEnum);
            
            if (provider == null)
            {
                _logger.LogWarning("Gateway {Gateway} not available", gatewayEnum);
                return BadRequest(new { message = $"Pasarela de pago {gatewayEnum} no disponible" });
            }

            string token;
            string cardBrand = "";
            string cardLast4 = "";
            int expMonth = 0;
            int expYear = 0;
            string? cardHolderName = null;

            // Si ya viene un token pre-generado, usarlo directamente
            if (!string.IsNullOrEmpty(request.Token))
            {
                token = request.Token;
                
                // Para tokens pre-generados, necesitamos la info de la tarjeta
                if (request.Card != null)
                {
                    cardBrand = DetectCardBrand(request.Card.Number);
                    cardLast4 = request.Card.Number.Length >= 4 
                        ? request.Card.Number.Substring(request.Card.Number.Length - 4) 
                        : "";
                    expMonth = request.Card.ExpMonth;
                    expYear = NormalizeYear(request.Card.ExpYear);
                    cardHolderName = request.Card.CardHolderName;
                }
            }
            else if (request.Card != null)
            {
                // Tokenizar la tarjeta con el proveedor
                var cardData = new CardData
                {
                    CardNumber = request.Card.Number,
                    ExpiryMonth = request.Card.ExpMonth.ToString(),
                    ExpiryYear = NormalizeYear(request.Card.ExpYear).ToString(),
                    CVV = request.Card.Cvv,
                    CardholderName = request.Card.CardHolderName ?? string.Empty
                };

                var tokenResult = await provider.TokenizeCardAsync(cardData, cancellationToken);
                
                if (!tokenResult.Success)
                {
                    _logger.LogWarning("Failed to tokenize card for user {UserId}: {Error}", 
                        userId, tokenResult.ErrorMessage);
                    return BadRequest(new 
                    { 
                        message = tokenResult.ErrorMessage ?? "Error al tokenizar la tarjeta",
                        code = "TOKENIZATION_FAILED"
                    });
                }

                token = tokenResult.Token!;
                cardBrand = tokenResult.CardBrand ?? DetectCardBrand(request.Card.Number);
                cardLast4 = tokenResult.CardLastFour ?? request.Card.Number.Substring(request.Card.Number.Length - 4);
                expMonth = request.Card.ExpMonth;
                expYear = NormalizeYear(request.Card.ExpYear);
                cardHolderName = request.Card.CardHolderName;
            }
            else
            {
                return BadRequest(new { message = "Se requieren datos de la tarjeta" });
            }

            // Verificar si ya existe una tarjeta similar (evitar duplicados)
            var exists = await _repository.ExistsSimilarAsync(userId, cardLast4, cardBrand, expMonth, expYear, cancellationToken);
            if (exists)
            {
                return Conflict(new 
                { 
                    message = "Ya tienes guardada esta tarjeta",
                    code = "DUPLICATE_CARD"
                });
            }

            // Crear el método de pago guardado
            var paymentMethod = new SavedPaymentMethod
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PaymentGateway = gatewayEnum,
                Token = token,
                Type = SavedPaymentMethodType.Card,
                NickName = request.NickName,
                IsDefault = request.SetAsDefault,
                CardBrand = cardBrand,
                CardLast4 = cardLast4,
                ExpirationMonth = expMonth,
                ExpirationYear = expYear,
                CardHolderName = cardHolderName
            };

            // Guardar dirección de facturación
            if (request.BillingAddress != null)
            {
                paymentMethod.BillingAddressJson = JsonSerializer.Serialize(request.BillingAddress);
            }

            var created = await _repository.CreateAsync(paymentMethod, cancellationToken);

            _logger.LogInformation("User {UserId} added payment method {MethodId} ({Brand} ****{Last4})", 
                userId, created.Id, cardBrand, cardLast4);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for user {UserId}", userId);
            return StatusCode(500, new { message = "Error al agregar el método de pago" });
        }
    }

    /// <summary>
    /// Actualiza un método de pago existente (solo nombre/dirección)
    /// </summary>
    /// <param name="id">ID del método de pago</param>
    /// <param name="request">Datos a actualizar</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var method = await _repository.GetByIdAndUserAsync(id, userId, cancellationToken);
        if (method == null)
        {
            return NotFound(new { message = "Método de pago no encontrado" });
        }

        // Solo actualizamos campos permitidos
        if (request.NickName != null)
        {
            method.NickName = request.NickName;
        }

        if (request.BillingAddress != null)
        {
            method.BillingAddressJson = JsonSerializer.Serialize(request.BillingAddress);
        }

        var updated = await _repository.UpdateAsync(method, cancellationToken);

        _logger.LogInformation("User {UserId} updated payment method {MethodId}", userId, id);

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Establece un método de pago como predeterminado
    /// </summary>
    /// <param name="id">ID del método de pago</param>
    [HttpPost("{id:guid}/default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        try
        {
            await _repository.SetAsDefaultAsync(id, userId, cancellationToken);

            _logger.LogInformation("User {UserId} set payment method {MethodId} as default", userId, id);

            return Ok(new { message = "Método de pago establecido como predeterminado" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Método de pago no encontrado" });
        }
    }

    /// <summary>
    /// Elimina un método de pago
    /// </summary>
    /// <param name="id">ID del método de pago</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        try
        {
            // Usamos soft delete (desactivar) para mantener historial
            await _repository.DeactivateAsync(id, userId, cancellationToken);

            _logger.LogInformation("User {UserId} deleted payment method {MethodId}", userId, id);

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Método de pago no encontrado" });
        }
    }

    /// <summary>
    /// Obtiene el método de pago predeterminado del usuario
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var method = await _repository.GetDefaultByUserIdAsync(userId, cancellationToken);
        if (method == null)
        {
            return NotFound(new { message = "No hay método de pago predeterminado" });
        }

        return Ok(method.ToDto());
    }

    // ==================== TOKENIZATION ENDPOINTS ====================

    /// <summary>
    /// Inicia el proceso de tokenización con un proveedor específico
    /// El usuario será redirigido a la página segura del proveedor
    /// </summary>
    /// <param name="request">Configuración de tokenización</param>
    [HttpPost("tokenize/init")]
    [ProducesResponseType(typeof(InitiateTokenizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateTokenization(
        [FromBody] InitiateTokenizationRequest request, 
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Validar límite de métodos de pago
        var currentCount = await _repository.CountByUserIdAsync(userId, cancellationToken);
        if (currentCount >= MaxPaymentMethodsPerUser)
        {
            return BadRequest(new 
            { 
                message = $"Has alcanzado el límite máximo de {MaxPaymentMethodsPerUser} métodos de pago",
                code = "MAX_PAYMENT_METHODS_REACHED"
            });
        }

        if (string.IsNullOrEmpty(request.ReturnUrl))
        {
            return BadRequest(new { message = "ReturnUrl es requerido" });
        }

        try
        {
            var response = await _tokenizationService.InitiateAsync(userId, request, cancellationToken);
            
            _logger.LogInformation(
                "User {UserId} initiated tokenization with {Gateway}, session: {SessionId}",
                userId, request.Gateway, response.SessionId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating tokenization for user {UserId}", userId);
            return StatusCode(500, new { message = "Error al iniciar tokenización" });
        }
    }

    /// <summary>
    /// Completa el proceso de tokenización después del callback del proveedor
    /// </summary>
    /// <param name="request">Datos de respuesta del proveedor</param>
    [HttpPost("tokenize/complete")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteTokenization(
        [FromBody] CompleteTokenizationRequest request, 
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        if (string.IsNullOrEmpty(request.SessionId))
        {
            return BadRequest(new { message = "SessionId es requerido" });
        }

        try
        {
            // Get session to verify ownership and get settings
            var session = await _tokenizationService.GetSessionAsync(request.SessionId, cancellationToken);
            if (session == null)
            {
                return BadRequest(new { message = "Sesión de tokenización no encontrada o expirada" });
            }

            // Complete tokenization to get the token
            var (token, cardBrand, cardLast4, expMonth, expYear, cardHolderName) = 
                await _tokenizationService.CompleteAsync(userId, request, cancellationToken);

            // Check for duplicates
            var exists = await _repository.ExistsSimilarAsync(
                userId, cardLast4, cardBrand, expMonth, expYear, cancellationToken);
            if (exists)
            {
                return Conflict(new 
                { 
                    message = "Ya tienes guardada esta tarjeta",
                    code = "DUPLICATE_CARD"
                });
            }

            // Create the saved payment method
            var paymentMethod = new SavedPaymentMethod
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PaymentGateway = session.Gateway,
                Token = token,
                Type = SavedPaymentMethodType.Card,
                NickName = session.NickName,
                IsDefault = session.SetAsDefault,
                CardBrand = cardBrand,
                CardLast4 = cardLast4,
                ExpirationMonth = expMonth,
                ExpirationYear = expYear,
                CardHolderName = cardHolderName
            };

            var created = await _repository.CreateAsync(paymentMethod, cancellationToken);

            _logger.LogInformation(
                "User {UserId} completed tokenization for payment method {MethodId} ({Brand} ****{Last4})", 
                userId, created.Id, cardBrand, cardLast4);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "La sesión no pertenece a este usuario" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing tokenization for user {UserId}", userId);
            return StatusCode(500, new { message = "Error al completar tokenización" });
        }
    }

    /// <summary>
    /// Obtiene la configuración de tokenización para un proveedor
    /// SEGURIDAD: Requiere autenticación para evitar information disclosure
    /// </summary>
    /// <param name="gateway">Nombre del proveedor</param>
    [HttpGet("tokenize/config/{gateway}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetTokenizationConfig(string gateway)
    {
        try
        {
            var gatewayEnum = DetermineGateway(gateway);
            var config = _tokenizationService.GetProviderConfig(gatewayEnum);
            
            // SEGURIDAD: No exponer IsTestMode a usuarios finales
            return Ok(new 
            {
                config.Gateway,
                config.IntegrationType,
                config.DisplayName,
                config.Description,
                config.SupportsVaulting,
                config.SupportedCardBrands
                // Intencionalmente omitido: IsTestMode
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el estado de una sesión de tokenización
    /// </summary>
    /// <param name="sessionId">ID de la sesión</param>
    [HttpGet("tokenize/session/{sessionId}")]
    [ProducesResponseType(typeof(TokenizationSession), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTokenizationSession(
        string sessionId, 
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var session = await _tokenizationService.GetSessionAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound(new { message = "Sesión no encontrada o expirada" });
        }

        if (session.UserId != userId)
        {
            return Unauthorized(new { message = "La sesión no pertenece a este usuario" });
        }

        return Ok(session);
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Obtiene el ID del usuario del token JWT
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                       ?? User.FindFirst("sub")
                       ?? User.FindFirst("userId");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Determina qué pasarela usar
    /// </summary>
    private PaymentGateway DetermineGateway(string? requestedGateway)
    {
        if (!string.IsNullOrEmpty(requestedGateway))
        {
            if (Enum.TryParse<PaymentGateway>(requestedGateway, true, out var gateway))
            {
                return gateway;
            }
        }

        // Por defecto usar Azul para República Dominicana
        return PaymentGateway.Azul;
    }

    /// <summary>
    /// Detecta la marca de la tarjeta basándose en el número
    /// </summary>
    private static string DetectCardBrand(string cardNumber)
    {
        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (string.IsNullOrEmpty(cleanNumber))
            return "Unknown";

        // Visa: Empieza con 4
        if (cleanNumber.StartsWith("4"))
            return "Visa";

        // Mastercard: Empieza con 51-55 o 2221-2720
        if (cleanNumber.Length >= 2)
        {
            var firstTwo = int.Parse(cleanNumber.Substring(0, 2));
            if (firstTwo >= 51 && firstTwo <= 55)
                return "Mastercard";
        }
        if (cleanNumber.Length >= 4)
        {
            var firstFour = int.Parse(cleanNumber.Substring(0, 4));
            if (firstFour >= 2221 && firstFour <= 2720)
                return "Mastercard";
        }

        // American Express: Empieza con 34 o 37
        if (cleanNumber.StartsWith("34") || cleanNumber.StartsWith("37"))
            return "Amex";

        // Discover: Empieza con 6011, 644-649, 65
        if (cleanNumber.StartsWith("6011") || cleanNumber.StartsWith("65"))
            return "Discover";
        if (cleanNumber.Length >= 3)
        {
            var firstThree = int.Parse(cleanNumber.Substring(0, 3));
            if (firstThree >= 644 && firstThree <= 649)
                return "Discover";
        }

        // Diners Club: Empieza con 36, 38, 300-305
        if (cleanNumber.StartsWith("36") || cleanNumber.StartsWith("38"))
            return "Diners";
        if (cleanNumber.Length >= 3)
        {
            var firstThree = int.Parse(cleanNumber.Substring(0, 3));
            if (firstThree >= 300 && firstThree <= 305)
                return "Diners";
        }

        return "Other";
    }

    /// <summary>
    /// Normaliza el año de expiración a 4 dígitos
    /// </summary>
    private static int NormalizeYear(int year)
    {
        if (year < 100)
        {
            return 2000 + year;
        }
        return year;
    }
}
