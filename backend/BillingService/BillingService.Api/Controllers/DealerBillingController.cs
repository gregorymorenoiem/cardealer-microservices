using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingService.Application.DTOs;
using BillingService.Application.Services;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;

namespace BillingService.Api.Controllers;

/// <summary>
/// Controlador para operaciones de billing específicas para dealers
/// Proporciona todos los endpoints que el frontend de dealer billing necesita
/// </summary>
[ApiController]
[Route("api/dealer-billing")]
public class DealerBillingController : ControllerBase
{
    private readonly BillingApplicationService _billingService;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<DealerBillingController> _logger;

    public DealerBillingController(
        BillingApplicationService billingService,
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ILogger<DealerBillingController> logger)
    {
        _billingService = billingService;
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    // ========================================
    // DASHBOARD ENDPOINTS
    // ========================================

    /// <summary>
    /// Obtiene todos los datos del dashboard de billing para un dealer
    /// </summary>
    [HttpGet("dashboard/{dealerId:guid}")]
    [AllowAnonymous] // TODO: Add proper auth
    public async Task<ActionResult<DealerBillingDashboardResponse>> GetDashboard(
        Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _billingService.GetBillingSummaryAsync(dealerId, cancellationToken);
            var plans = await _billingService.GetPlanPricingAsync(dealerId, cancellationToken);
            
            // Calcular usage metrics
            var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
            var usage = await CalculateUsageMetrics(dealerId, subscription, cancellationToken);
            var stats = await CalculateBillingStats(dealerId, subscription, cancellationToken);

            return Ok(new DealerBillingDashboardResponse(
                Summary: summary,
                Plans: plans,
                Usage: usage,
                Stats: stats
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing dashboard for dealer {DealerId}", dealerId);
            return StatusCode(500, new { error = "Error fetching billing dashboard" });
        }
    }

    /// <summary>
    /// Obtiene la suscripción actual de un dealer
    /// </summary>
    [HttpGet("subscription")]
    [AllowAnonymous]
    public async Task<ActionResult<DealerSubscriptionDto>> GetSubscription(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        if (subscription == null)
        {
            // Return a default free subscription info
            return Ok(new DealerSubscriptionDto(
                Id: Guid.Empty.ToString(),
                DealerId: dealerId.ToString(),
                Plan: "free",
                Status: "active",
                Cycle: "monthly",
                PricePerCycle: 0,
                Currency: "USD",
                StartDate: DateTime.UtcNow.ToString("o"),
                NextBillingDate: null,
                MaxUsers: 1,
                MaxVehicles: 3,
                Features: GetDefaultFeatures("free")
            ));
        }

        return Ok(MapToSubscriptionDto(subscription));
    }

    /// <summary>
    /// Obtiene las facturas de un dealer
    /// </summary>
    [HttpGet("invoices")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DealerInvoiceDto>>> GetInvoices(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return Ok(invoices.Select(MapToInvoiceDto).ToList());
    }

    /// <summary>
    /// Obtiene los pagos de un dealer
    /// </summary>
    [HttpGet("payments")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DealerPaymentDto>>> GetPayments(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return Ok(payments.Select(MapToPaymentDto).ToList());
    }

    /// <summary>
    /// Obtiene métricas de uso para un dealer
    /// </summary>
    [HttpGet("usage")]
    [AllowAnonymous]
    public async Task<ActionResult<UsageMetricsDto>> GetUsage(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var usage = await CalculateUsageMetrics(dealerId, subscription, cancellationToken);
        return Ok(usage);
    }

    /// <summary>
    /// Obtiene estadísticas de billing para un dealer
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<ActionResult<BillingStatsDto>> GetStats(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var stats = await CalculateBillingStats(dealerId, subscription, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Obtiene todos los planes disponibles
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PlanConfigDto>>> GetPlans(
        [FromHeader(Name = "X-Dealer-Id")] Guid? dealerId = null,
        CancellationToken cancellationToken = default)
    {
        var plans = await _billingService.GetPlanPricingAsync(dealerId, cancellationToken);
        return Ok(plans.Select(MapToPlanConfigDto).ToList());
    }

    /// <summary>
    /// Obtiene los métodos de pago de un dealer
    /// </summary>
    [HttpGet("payment-methods")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetPaymentMethods(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        // For now, return mock data - in production, this would come from Stripe
        var customer = await _billingService.GetCustomerByDealerIdAsync(dealerId, cancellationToken);
        
        if (customer?.PaymentMethods != null && customer.PaymentMethods.Any())
        {
            return Ok(customer.PaymentMethods.Select(pm => new PaymentMethodDto(
                Id: pm.Id,
                Type: pm.Type == "card" ? "card" : "bank_account",
                IsDefault: pm.IsDefault,
                Card: pm.Type == "card" ? new CardInfoDto(
                    Brand: pm.Brand ?? "Unknown",
                    Last4: pm.Last4 ?? "****",
                    ExpMonth: pm.ExpMonth ?? 12,
                    ExpYear: pm.ExpYear ?? 2025
                ) : null,
                BankAccount: null,
                CreatedAt: DateTime.UtcNow.ToString("o")
            )).ToList());
        }

        // Return empty list if no payment methods
        return Ok(new List<PaymentMethodDto>());
    }

    /// <summary>
    /// Add a new payment method (card or bank account)
    /// </summary>
    [HttpPost("payment-methods")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodDto>> AddPaymentMethod(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromBody] AddPaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding payment method for dealer {DealerId}", dealerId);
        
        // In production, this would:
        // 1. Call Stripe/Azul to create the payment method
        // 2. Store the token reference in our database
        // 3. Return the created payment method
        
        var newPaymentMethod = new PaymentMethodDto(
            Id: Guid.NewGuid().ToString(),
            Type: request.Type,
            IsDefault: request.SetAsDefault,
            Card: request.Type == "card" ? new CardInfoDto(
                Brand: "Visa", // Would come from payment processor
                Last4: "****",
                ExpMonth: request.ExpMonth ?? 12,
                ExpYear: request.ExpYear ?? DateTime.UtcNow.Year + 3
            ) : null,
            BankAccount: null,
            CreatedAt: DateTime.UtcNow.ToString("o")
        );
        
        return Ok(newPaymentMethod);
    }

    /// <summary>
    /// Set a payment method as default
    /// </summary>
    [HttpPut("payment-methods/{paymentMethodId}/default")]
    [AllowAnonymous]
    public async Task<ActionResult> SetDefaultPaymentMethod(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromRoute] string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting default payment method {PaymentMethodId} for dealer {DealerId}",
            paymentMethodId, dealerId);
        
        // In production, this would:
        // 1. Update the default payment method in Stripe/Azul
        // 2. Update our database records
        
        return Ok(new { Success = true, Message = "Default payment method updated" });
    }

    /// <summary>
    /// Remove a payment method
    /// </summary>
    [HttpDelete("payment-methods/{paymentMethodId}")]
    [AllowAnonymous]
    public async Task<ActionResult> RemovePaymentMethod(
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromRoute] string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing payment method {PaymentMethodId} for dealer {DealerId}",
            paymentMethodId, dealerId);
        
        // In production, this would:
        // 1. Detach the payment method from Stripe/Azul
        // 2. Remove from our database
        // 3. Verify it's not the only payment method if subscription is active
        
        return Ok(new { Success = true, Message = "Payment method removed" });
    }

    // ========================================
    // PRIVATE HELPER METHODS
    // ========================================


    private async Task<UsageMetricsDto> CalculateUsageMetrics(
        Guid dealerId,
        Subscription? subscription,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual usage from VehiclesSaleService
        // For now, return sample data based on plan
        var plan = subscription?.Plan ?? SubscriptionPlan.Free;
        var (maxUsers, maxVehicles) = StripePriceMapping.GetLimits(plan);

        // Simulated usage - in production, query actual vehicle count from VehiclesSaleService
        var currentListings = plan switch
        {
            SubscriptionPlan.Free => 2,
            SubscriptionPlan.Basic => 25,
            SubscriptionPlan.Professional => 150,
            SubscriptionPlan.Enterprise => 500,
            _ => 0
        };

        var currentUsers = plan switch
        {
            SubscriptionPlan.Free => 1,
            SubscriptionPlan.Basic => 3,
            SubscriptionPlan.Professional => 10,
            SubscriptionPlan.Enterprise => 25,
            _ => 1
        };

        return new UsageMetricsDto(
            CurrentListings: currentListings,
            MaxListings: maxVehicles == -1 ? "unlimited" : maxVehicles.ToString(),
            CurrentUsers: currentUsers,
            MaxUsers: maxUsers == -1 ? "unlimited" : maxUsers.ToString(),
            StorageUsed: "2.5 GB",
            StorageLimit: plan == SubscriptionPlan.Enterprise ? "unlimited" : "10 GB",
            ApiCalls: 15420,
            ApiLimit: plan == SubscriptionPlan.Enterprise ? "unlimited" : "50000"
        );
    }

    private async Task<BillingStatsDto> CalculateBillingStats(
        Guid dealerId,
        Subscription? subscription,
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var payments = await _paymentRepository.GetByDealerIdAsync(dealerId, cancellationToken);

        var currentYear = DateTime.UtcNow.Year;
        var yearlyPayments = payments
            .Where(p => p.Status == PaymentStatus.Succeeded && p.CreatedAt.Year == currentYear)
            .ToList();

        var monthlySpend = yearlyPayments
            .Where(p => p.CreatedAt.Month == DateTime.UtcNow.Month)
            .Sum(p => p.Amount);

        var yearlySpend = yearlyPayments.Sum(p => p.Amount);

        var outstandingBalance = invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided)
            .Sum(i => i.TotalAmount - i.PaidAmount);

        var totalPaid = payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .Sum(p => p.Amount);

        return new BillingStatsDto(
            CurrentPlan: subscription?.Plan.ToString().ToLower() ?? "free",
            MonthlySpend: monthlySpend,
            YearlySpend: yearlySpend,
            OutstandingBalance: outstandingBalance,
            NextBillingAmount: subscription?.PricePerCycle ?? 0,
            NextBillingDate: subscription?.NextBillingDate?.ToString("o") ?? DateTime.UtcNow.AddMonths(1).ToString("o"),
            TotalPaid: totalPaid,
            InvoiceCount: invoices.Count()
        );
    }

    private DealerSubscriptionDto MapToSubscriptionDto(Subscription sub)
    {
        return new DealerSubscriptionDto(
            Id: sub.Id.ToString(),
            DealerId: sub.DealerId.ToString(),
            Plan: sub.Plan.ToString().ToLower(),
            Status: sub.Status.ToString().ToLower(),
            Cycle: sub.Cycle.ToString().ToLower(),
            PricePerCycle: sub.PricePerCycle,
            Currency: sub.Currency,
            StartDate: sub.StartDate.ToString("o"),
            NextBillingDate: sub.NextBillingDate?.ToString("o"),
            MaxUsers: sub.MaxUsers,
            MaxVehicles: sub.MaxVehicles,
            Features: GetDefaultFeatures(sub.Plan.ToString().ToLower())
        );
    }

    private DealerInvoiceDto MapToInvoiceDto(Invoice inv)
    {
        return new DealerInvoiceDto(
            Id: inv.Id.ToString(),
            DealerId: inv.DealerId.ToString(),
            InvoiceNumber: inv.InvoiceNumber,
            SubscriptionId: inv.SubscriptionId?.ToString(),
            Status: inv.Status.ToString().ToLower().Replace("_", "-"),
            Subtotal: inv.Subtotal,
            TaxAmount: inv.TaxAmount,
            TotalAmount: inv.TotalAmount,
            PaidAmount: inv.PaidAmount,
            Currency: inv.Currency,
            IssueDate: inv.IssueDate.ToString("o"),
            DueDate: inv.DueDate.ToString("o"),
            PaidDate: inv.PaidDate?.ToString("o"),
            PdfUrl: inv.PdfUrl
        );
    }

    private DealerPaymentDto MapToPaymentDto(Payment pay)
    {
        return new DealerPaymentDto(
            Id: pay.Id.ToString(),
            DealerId: pay.DealerId.ToString(),
            SubscriptionId: pay.SubscriptionId?.ToString(),
            InvoiceId: pay.InvoiceId?.ToString(),
            Amount: pay.Amount,
            Currency: pay.Currency,
            Status: pay.Status.ToString().ToLower(),
            Method: pay.Method.ToString().ToLower().Replace("_", "-"),
            Description: pay.Description,
            ReceiptUrl: pay.ReceiptUrl,
            RefundedAmount: pay.RefundedAmount,
            CreatedAt: pay.CreatedAt.ToString("o"),
            ProcessedAt: pay.ProcessedAt?.ToString("o"),
            CardLast4: "4242", // In production, get from Stripe
            CardBrand: "Visa"  // In production, get from Stripe
        );
    }

    private PlanConfigDto MapToPlanConfigDto(PlanPricingInfo plan)
    {
        return new PlanConfigDto(
            Id: plan.Plan.ToString().ToLower(),
            Name: plan.PlanName,
            Description: GetPlanDescription(plan.Plan),
            Prices: new PlanPricesDto(
                Monthly: plan.MonthlyPrice,
                Quarterly: plan.MonthlyPrice * 3 * 0.9m, // 10% discount for quarterly
                Yearly: plan.YearlyPrice
            ),
            Features: GetDefaultFeatures(plan.Plan.ToString().ToLower()),
            Popular: plan.Plan == SubscriptionPlan.Professional,
            Enterprise: plan.Plan == SubscriptionPlan.Enterprise
        );
    }

    private string GetPlanDescription(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Free => "Para empezar tu negocio",
        SubscriptionPlan.Basic => "Para dealers en crecimiento",
        SubscriptionPlan.Professional => "Para dealers establecidos",
        SubscriptionPlan.Enterprise => "Para grandes operaciones",
        _ => "Plan personalizado"
    };

    private FeaturesDto GetDefaultFeatures(string plan) => plan.ToLower() switch
    {
        "free" => new FeaturesDto(3, 1, "1 GB", false, false, false, false, false, false, false, false, false, false),
        "basic" => new FeaturesDto(50, 5, "5 GB", true, false, false, true, false, true, true, false, true, true),
        "professional" => new FeaturesDto(500, 20, "20 GB", true, true, true, true, true, true, true, true, true, true),
        "enterprise" => new FeaturesDto(-1, -1, "unlimited", true, true, true, true, true, true, true, true, true, true),
        _ => new FeaturesDto(3, 1, "1 GB", false, false, false, false, false, false, false, false, false, false)
    };
}

// ========================================
// DTOs for Dealer Billing
// ========================================

public record DealerBillingDashboardResponse(
    BillingSummaryResponse Summary,
    List<PlanPricingInfo> Plans,
    UsageMetricsDto Usage,
    BillingStatsDto Stats
);

public record DealerSubscriptionDto(
    string Id,
    string DealerId,
    string Plan,
    string Status,
    string Cycle,
    decimal PricePerCycle,
    string Currency,
    string StartDate,
    string? NextBillingDate,
    int MaxUsers,
    int MaxVehicles,
    FeaturesDto Features
);

public record DealerInvoiceDto(
    string Id,
    string DealerId,
    string InvoiceNumber,
    string? SubscriptionId,
    string Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    string Currency,
    string IssueDate,
    string DueDate,
    string? PaidDate,
    string? PdfUrl
);

public record DealerPaymentDto(
    string Id,
    string DealerId,
    string? SubscriptionId,
    string? InvoiceId,
    decimal Amount,
    string Currency,
    string Status,
    string Method,
    string? Description,
    string? ReceiptUrl,
    decimal RefundedAmount,
    string CreatedAt,
    string? ProcessedAt,
    string? CardLast4,
    string? CardBrand
);

public record UsageMetricsDto(
    int CurrentListings,
    string MaxListings,
    int CurrentUsers,
    string MaxUsers,
    string StorageUsed,
    string StorageLimit,
    int ApiCalls,
    string ApiLimit
);

public record BillingStatsDto(
    string CurrentPlan,
    decimal MonthlySpend,
    decimal YearlySpend,
    decimal OutstandingBalance,
    decimal NextBillingAmount,
    string NextBillingDate,
    decimal TotalPaid,
    int InvoiceCount
);

public record PlanConfigDto(
    string Id,
    string Name,
    string Description,
    PlanPricesDto Prices,
    FeaturesDto Features,
    bool Popular,
    bool Enterprise
);

public record PlanPricesDto(
    decimal Monthly,
    decimal Quarterly,
    decimal Yearly
);

public record FeaturesDto(
    int Listings,
    int Users,
    string Storage,
    bool Analytics,
    bool Api,
    bool CustomBranding,
    bool PrioritySupport,
    bool DedicatedManager,
    bool BulkUpload,
    bool Marketplace,
    bool RealEstate,
    bool Crm,
    bool Reporting
);

public record PaymentMethodDto(
    string Id,
    string Type,
    bool IsDefault,
    CardInfoDto? Card,
    BankAccountInfoDto? BankAccount,
    string CreatedAt
);

public record CardInfoDto(
    string Brand,
    string Last4,
    int ExpMonth,
    int ExpYear
);

public record BankAccountInfoDto(
    string BankName,
    string Last4,
    string AccountType
);
