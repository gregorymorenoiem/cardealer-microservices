using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingService.Domain.Entities;

namespace BillingService.Api.Controllers;

/// <summary>
/// Controlador para el sistema de créditos OKLA Coins.
/// Permite comprar paquetes, consultar balance, y ver historial de transacciones.
/// </summary>
[ApiController]
[Route("api/okla-coins")]
[Authorize]
public class OklaCoinsController : ControllerBase
{
    private readonly ILogger<OklaCoinsController> _logger;

    public OklaCoinsController(ILogger<OklaCoinsController> logger)
    {
        _logger = logger;
    }

    // ========================================
    // PACKAGES (Public)
    // ========================================

    /// <summary>
    /// Obtiene todos los paquetes de OKLA Coins disponibles
    /// </summary>
    [HttpGet("packages")]
    [AllowAnonymous]
    public ActionResult<CoinsPackagesResponse> GetPackages()
    {
        var packages = OklaCoinPackageCatalog.GetAll()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new CoinPackageDto(
                Id: p.Id.ToString(),
                Slug: p.Slug,
                Name: p.Name,
                BaseCredits: p.BaseCredits,
                BonusCredits: p.BonusCredits,
                TotalCredits: p.TotalCredits,
                BonusPercentage: p.BonusPercentage,
                PriceUsd: p.PriceUsd,
                CostPerCoin: Math.Round(p.CostPerCoin, 5),
                BadgeText: p.BadgeText
            ))
            .ToList();

        return Ok(new CoinsPackagesResponse(
            Packages: packages,
            ConversionRate: "1 OKLA Coin = $0.01 USD",
            TotalPackages: packages.Count
        ));
    }

    // ========================================
    // WALLET
    // ========================================

    /// <summary>
    /// Obtiene el balance de OKLA Coins del dealer
    /// </summary>
    [HttpGet("wallet")]
    public ActionResult<WalletDto> GetWallet(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId)
    {
        _logger.LogInformation("Getting OKLA Coins wallet for dealer {DealerId}", dealerId);

        // In production, this would query the database
        // For now, return a demo wallet
        var wallet = new WalletDto(
            DealerId: dealerId.ToString(),
            Balance: 5500,
            TotalPurchased: 5000,
            TotalBonus: 500,
            TotalFromPlan: 0,
            TotalSpent: 0,
            Currency: "USD",
            BalanceUsd: 55.00m,
            LastPlanCreditDate: null
        );

        return Ok(wallet);
    }

    /// <summary>
    /// Compra un paquete de OKLA Coins
    /// </summary>
    [HttpPost("purchase")]
    public ActionResult<CoinPurchaseResult> PurchasePackage(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromBody] PurchasePackageRequest request)
    {
        var package = OklaCoinPackageCatalog.GetBySlug(request.PackageSlug);
        if (package == null)
            return NotFound(new { error = $"Package '{request.PackageSlug}' not found" });

        _logger.LogInformation(
            "Dealer {DealerId} purchasing OKLA Coins package {PackageSlug} ({TotalCredits} coins for ${Price})",
            dealerId, package.Slug, package.TotalCredits, package.PriceUsd);

        // In production:
        // 1. Create payment via Stripe/Azul
        // 2. On payment success, credit wallet
        // 3. Record transactions (base + bonus)
        // 4. Publish billing.credits.purchased event

        var transactionId = Guid.NewGuid();
        var result = new CoinPurchaseResult(
            TransactionId: transactionId.ToString(),
            PackageSlug: package.Slug,
            PackageName: package.Name,
            BaseCredits: package.BaseCredits,
            BonusCredits: package.BonusCredits,
            TotalCredits: package.TotalCredits,
            AmountCharged: package.PriceUsd,
            Currency: "USD",
            NewBalance: 5500 + package.TotalCredits, // Demo: add to existing balance
            Status: "completed",
            Message: $"¡{package.TotalCredits:N0} OKLA Coins acreditados exitosamente!"
        );

        return Ok(result);
    }

    /// <summary>
    /// Gasta OKLA Coins en un producto publicitario
    /// </summary>
    [HttpPost("spend")]
    public ActionResult<CoinSpendResult> SpendCoins(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromBody] SpendCoinsRequest request)
    {
        _logger.LogInformation(
            "Dealer {DealerId} spending {Amount} OKLA Coins on product {ProductSlug}",
            dealerId, request.Amount, request.ProductSlug);

        // In production:
        // 1. Validate sufficient balance
        // 2. Debit wallet
        // 3. Create ad campaign or activate product
        // 4. Record transaction
        
        if (request.Amount <= 0)
            return BadRequest(new { error = "Amount must be positive" });

        var result = new CoinSpendResult(
            TransactionId: Guid.NewGuid().ToString(),
            AmountSpent: request.Amount,
            ProductSlug: request.ProductSlug,
            NewBalance: Math.Max(0, 5500 - request.Amount),
            Status: "completed",
            Message: $"{request.Amount:N0} OKLA Coins utilizados exitosamente"
        );

        return Ok(result);
    }

    // ========================================
    // TRANSACTIONS
    // ========================================

    /// <summary>
    /// Obtiene el historial de transacciones de OKLA Coins
    /// </summary>
    [HttpGet("transactions")]
    public ActionResult<CoinsTransactionsResponse> GetTransactions(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting OKLA Coins transactions for dealer {DealerId}", dealerId);

        // Demo transactions
        var transactions = new List<CoinTransactionDto>
        {
            new(
                Id: Guid.NewGuid().ToString(),
                Type: "purchase",
                Amount: 5000,
                BalanceAfter: 5000,
                Description: "Compra Pack Intermedio",
                PackageSlug: "pack-intermedio",
                AmountUsd: 50.00m,
                CreatedAt: DateTime.UtcNow.AddDays(-5).ToString("o")
            ),
            new(
                Id: Guid.NewGuid().ToString(),
                Type: "bonus",
                Amount: 500,
                BalanceAfter: 5500,
                Description: "Bonus 10% - Pack Intermedio",
                PackageSlug: "pack-intermedio",
                AmountUsd: null,
                CreatedAt: DateTime.UtcNow.AddDays(-5).ToString("o")
            )
        };

        return Ok(new CoinsTransactionsResponse(
            Transactions: transactions,
            TotalCount: transactions.Count,
            Page: page,
            PageSize: pageSize
        ));
    }

    /// <summary>
    /// Admin: Acredita OKLA Coins mensuales del plan a un dealer
    /// </summary>
    [HttpPost("admin/plan-credit")]
    [Authorize(Roles = "admin")]
    public ActionResult<CoinPurchaseResult> CreditPlanCoins(
        [FromBody] PlanCreditRequest request)
    {
        _logger.LogInformation(
            "Admin crediting {Amount} OKLA Coins to dealer {DealerId} from plan",
            request.CoinsAmount, request.DealerId);

        var result = new CoinPurchaseResult(
            TransactionId: Guid.NewGuid().ToString(),
            PackageSlug: "plan-credit",
            PackageName: "Créditos mensuales del plan",
            BaseCredits: request.CoinsAmount,
            BonusCredits: 0,
            TotalCredits: request.CoinsAmount,
            AmountCharged: 0m,
            Currency: "USD",
            NewBalance: 5500 + request.CoinsAmount,
            Status: "completed",
            Message: $"¡{request.CoinsAmount:N0} OKLA Coins del plan acreditados!"
        );

        return Ok(result);
    }
}

// ── DTOs ──

public record CoinsPackagesResponse(
    List<CoinPackageDto> Packages,
    string ConversionRate,
    int TotalPackages
);

public record CoinPackageDto(
    string Id,
    string Slug,
    string Name,
    int BaseCredits,
    int BonusCredits,
    int TotalCredits,
    int BonusPercentage,
    decimal PriceUsd,
    decimal CostPerCoin,
    string? BadgeText
);

public record WalletDto(
    string DealerId,
    int Balance,
    int TotalPurchased,
    int TotalBonus,
    int TotalFromPlan,
    int TotalSpent,
    string Currency,
    decimal BalanceUsd,
    string? LastPlanCreditDate
);

public record PurchasePackageRequest(
    string PackageSlug,
    string? PaymentMethodId = null
);

public record CoinPurchaseResult(
    string TransactionId,
    string PackageSlug,
    string PackageName,
    int BaseCredits,
    int BonusCredits,
    int TotalCredits,
    decimal AmountCharged,
    string Currency,
    int NewBalance,
    string Status,
    string Message
);

public record SpendCoinsRequest(
    string ProductSlug,
    int Amount,
    Guid? VehicleId = null,
    string? Duration = "month"
);

public record CoinSpendResult(
    string TransactionId,
    int AmountSpent,
    string ProductSlug,
    int NewBalance,
    string Status,
    string Message
);

public record CoinsTransactionsResponse(
    List<CoinTransactionDto> Transactions,
    int TotalCount,
    int Page,
    int PageSize
);

public record CoinTransactionDto(
    string Id,
    string Type,
    int Amount,
    int BalanceAfter,
    string Description,
    string? PackageSlug,
    decimal? AmountUsd,
    string CreatedAt
);

public record PlanCreditRequest(
    Guid DealerId,
    int CoinsAmount
);
