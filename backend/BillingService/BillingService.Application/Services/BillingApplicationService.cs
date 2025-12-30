using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;

namespace BillingService.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de billing con Stripe
/// </summary>
public class BillingApplicationService
{
    private readonly IStripeService _stripeService;
    private readonly IStripeCustomerRepository _customerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public BillingApplicationService(
        IStripeService stripeService,
        IStripeCustomerRepository customerRepository,
        ISubscriptionRepository subscriptionRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _stripeService = stripeService;
        _customerRepository = customerRepository;
        _subscriptionRepository = subscriptionRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    // ========================================
    // CUSTOMER OPERATIONS
    // ========================================

    /// <summary>
    /// Crea un cliente de Stripe para un dealer
    /// </summary>
    public async Task<StripeCustomerResponse> CreateCustomerAsync(
        CreateStripeCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verificar si ya existe
        var existingCustomer = await _customerRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        if (existingCustomer != null)
        {
            return MapToCustomerResponse(existingCustomer);
        }

        // Crear en Stripe
        var stripeResult = await _stripeService.CreateCustomerAsync(
            request.Email,
            request.Name,
            request.Phone,
            request.DealerId.ToString(),
            request.Metadata,
            cancellationToken);

        // Crear en base de datos
        var customer = new StripeCustomer(
            request.DealerId,
            stripeResult.Id,
            request.Email,
            request.Name,
            request.Phone
        );

        await _customerRepository.AddAsync(customer, cancellationToken);

        return MapToCustomerResponse(customer);
    }

    /// <summary>
    /// Obtiene el cliente de Stripe de un dealer
    /// </summary>
    public async Task<StripeCustomerResponse?> GetCustomerByDealerIdAsync(
        Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return customer != null ? MapToCustomerResponse(customer) : null;
    }

    /// <summary>
    /// Adjunta un método de pago al cliente
    /// </summary>
    public async Task<StripeCustomerResponse> AttachPaymentMethodAsync(
        AttachPaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer not found for dealer {request.DealerId}");
        }

        await _stripeService.AttachPaymentMethodAsync(
            request.PaymentMethodId,
            customer.StripeCustomerId,
            cancellationToken);

        if (request.SetAsDefault)
        {
            await _stripeService.SetDefaultPaymentMethodAsync(
                customer.StripeCustomerId,
                request.PaymentMethodId,
                cancellationToken);

            customer.SetDefaultPaymentMethod(request.PaymentMethodId);
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        return MapToCustomerResponse(customer);
    }

    // ========================================
    // SUBSCRIPTION OPERATIONS
    // ========================================

    /// <summary>
    /// Crea una nueva suscripción para un dealer
    /// </summary>
    public async Task<SubscriptionResponse> CreateSubscriptionAsync(
        CreateStripeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Obtener o crear cliente
        var customer = await _customerRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer not found for dealer {request.DealerId}. Create customer first.");
        }

        // Obtener el Price ID de Stripe
        var priceId = StripePriceMapping.GetPriceId(request.Plan, request.Cycle);
        if (string.IsNullOrEmpty(priceId) && request.Plan != SubscriptionPlan.Free)
        {
            throw new InvalidOperationException($"Price ID not configured for plan {request.Plan} with cycle {request.Cycle}");
        }

        // Si es Free, solo crear en local
        if (request.Plan == SubscriptionPlan.Free)
        {
            return await CreateFreeSubscriptionAsync(request.DealerId, cancellationToken);
        }

        // Crear suscripción en Stripe
        var trialDays = request.EnableTrial ? request.TrialDays : 0;
        var stripeResult = await _stripeService.CreateSubscriptionAsync(
            customer.StripeCustomerId,
            priceId,
            trialDays,
            null,
            cancellationToken);

        // Obtener límites según el plan
        var (maxUsers, maxVehicles) = StripePriceMapping.GetLimits(request.Plan);
        var price = StripePriceMapping.GetPrice(request.Plan, request.Cycle);

        // Crear suscripción en base de datos
        var subscription = new Subscription(
            request.DealerId,
            request.Plan,
            request.Cycle,
            price,
            maxUsers,
            maxVehicles,
            trialDays
        );

        subscription.SetStripeInfo(customer.StripeCustomerId, stripeResult.Id);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        return MapToSubscriptionResponse(subscription);
    }

    /// <summary>
    /// Actualiza una suscripción (upgrade/downgrade)
    /// </summary>
    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(
        UpdateStripeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {request.SubscriptionId} not found");
        }

        var newCycle = request.NewCycle ?? subscription.Cycle;
        var priceId = StripePriceMapping.GetPriceId(request.NewPlan, newCycle);

        // Actualizar en Stripe si existe
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            await _stripeService.UpdateSubscriptionAsync(
                subscription.StripeSubscriptionId,
                priceId,
                request.ProrationEnabled,
                cancellationToken);
        }

        // Actualizar en local
        var (maxUsers, maxVehicles) = StripePriceMapping.GetLimits(request.NewPlan);
        var newPrice = StripePriceMapping.GetPrice(request.NewPlan, newCycle);

        subscription.Upgrade(request.NewPlan, newPrice, maxUsers, maxVehicles);
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        return MapToSubscriptionResponse(subscription);
    }

    /// <summary>
    /// Cancela una suscripción
    /// </summary>
    public async Task<SubscriptionResponse> CancelSubscriptionAsync(
        CancelStripeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription == null)
        {
            throw new InvalidOperationException($"Subscription {request.SubscriptionId} not found");
        }

        // Cancelar en Stripe si existe
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            await _stripeService.CancelSubscriptionAsync(
                subscription.StripeSubscriptionId,
                request.CancelImmediately,
                cancellationToken);
        }

        // Cancelar en local
        subscription.Cancel(request.Reason ?? "No reason provided");
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        return MapToSubscriptionResponse(subscription);
    }

    /// <summary>
    /// Obtiene la suscripción actual de un dealer
    /// </summary>
    public async Task<SubscriptionResponse?> GetSubscriptionByDealerIdAsync(
        Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        return subscription != null ? MapToSubscriptionResponse(subscription) : null;
    }

    // ========================================
    // CHECKOUT & BILLING PORTAL
    // ========================================

    /// <summary>
    /// Crea una sesión de Stripe Checkout para nueva suscripción
    /// </summary>
    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Obtener o crear cliente
        var customer = await _customerRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer not found for dealer {request.DealerId}. Create customer first.");
        }

        var priceId = StripePriceMapping.GetPriceId(request.Plan, request.Cycle);
        var trialDays = request.EnableTrial ? request.TrialDays : 0;

        var checkoutResult = await _stripeService.CreateCheckoutSessionAsync(
            customer.StripeCustomerId,
            priceId,
            request.SuccessUrl,
            request.CancelUrl,
            trialDays,
            null,
            cancellationToken);

        return new CheckoutSessionResponse(checkoutResult.SessionId, checkoutResult.Url);
    }

    /// <summary>
    /// Crea una sesión del portal de billing
    /// </summary>
    public async Task<BillingPortalSessionResponse> CreateBillingPortalSessionAsync(
        CreateBillingPortalSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer not found for dealer {request.DealerId}");
        }

        var portalResult = await _stripeService.CreateBillingPortalSessionAsync(
            customer.StripeCustomerId,
            request.ReturnUrl,
            cancellationToken);

        return new BillingPortalSessionResponse("", portalResult.Url);
    }

    // ========================================
    // BILLING SUMMARY
    // ========================================

    /// <summary>
    /// Obtiene el resumen de billing para un dealer
    /// </summary>
    public async Task<BillingSummaryResponse> GetBillingSummaryAsync(
        Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var invoices = await _invoiceRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        var payments = await _paymentRepository.GetByDealerIdAsync(dealerId, cancellationToken);

        var recentInvoices = invoices
            .OrderByDescending(i => i.IssueDate)
            .Take(10)
            .Select(MapToInvoiceResponse)
            .ToList();

        var recentPayments = payments
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(MapToPaymentResponse)
            .ToList();

        var totalPaid = payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .Sum(p => p.Amount);

        var outstandingBalance = invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.TotalAmount - i.PaidAmount);

        return new BillingSummaryResponse(
            subscription != null ? MapToSubscriptionResponse(subscription) : null,
            customer != null ? MapToCustomerResponse(customer) : null,
            recentInvoices,
            recentPayments,
            totalPaid,
            outstandingBalance,
            subscription?.NextBillingDate,
            subscription?.PricePerCycle
        );
    }

    /// <summary>
    /// Obtiene los precios de todos los planes
    /// </summary>
    public Task<List<PlanPricingInfo>> GetPlanPricingAsync(
        Guid? dealerId = null,
        CancellationToken cancellationToken = default)
    {
        SubscriptionPlan? currentPlan = null;

        if (dealerId.HasValue)
        {
            var subscription = _subscriptionRepository.GetByDealerIdAsync(dealerId.Value, cancellationToken).Result;
            currentPlan = subscription?.Plan;
        }

        return Task.FromResult(StripePriceMapping.GetAllPricing(currentPlan));
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private async Task<SubscriptionResponse> CreateFreeSubscriptionAsync(
        Guid dealerId,
        CancellationToken cancellationToken)
    {
        var (maxUsers, maxVehicles) = StripePriceMapping.GetLimits(SubscriptionPlan.Free);

        var subscription = new Subscription(
            dealerId,
            SubscriptionPlan.Free,
            BillingCycle.Monthly,
            0m,
            maxUsers,
            maxVehicles,
            0
        );

        subscription.Activate();
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        return MapToSubscriptionResponse(subscription);
    }

    private static StripeCustomerResponse MapToCustomerResponse(StripeCustomer customer)
    {
        // Simplificado - no tenemos lista de payment methods en la entidad
        var paymentMethods = new List<PaymentMethodInfo>();

        if (!string.IsNullOrEmpty(customer.DefaultPaymentMethodId))
        {
            paymentMethods.Add(new PaymentMethodInfo(
                customer.DefaultPaymentMethodId,
                "card",
                null,
                null,
                null,
                null,
                true
            ));
        }

        return new StripeCustomerResponse(
            customer.Id,
            customer.DealerId,
            customer.StripeCustomerId,
            customer.Email,
            customer.Name,
            customer.Phone,
            customer.DefaultPaymentMethodId,
            paymentMethods,
            customer.IsActive,
            customer.CreatedAt
        );
    }

    private static SubscriptionResponse MapToSubscriptionResponse(Subscription subscription)
    {
        return new SubscriptionResponse(
            subscription.Id,
            subscription.DealerId,
            subscription.Plan,
            subscription.Status,
            subscription.Cycle,
            subscription.PricePerCycle,
            subscription.Currency,
            subscription.StartDate,
            subscription.EndDate,
            subscription.TrialEndDate,
            subscription.NextBillingDate,
            subscription.StripeSubscriptionId,
            subscription.MaxUsers,
            subscription.MaxVehicles,
            subscription.CreatedAt
        );
    }

    private static InvoiceResponse MapToInvoiceResponse(Invoice invoice)
    {
        return new InvoiceResponse(
            invoice.Id,
            invoice.DealerId,
            invoice.InvoiceNumber,
            invoice.Status.ToString(),
            invoice.Subtotal,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.PaidDate,
            invoice.PdfUrl
        );
    }

    private static PaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.DealerId,
            payment.Amount,
            payment.Status.ToString(),
            payment.Method.ToString(),
            payment.Description,
            payment.ReceiptUrl,
            payment.ProcessedAt,
            payment.CreatedAt
        );
    }
}
