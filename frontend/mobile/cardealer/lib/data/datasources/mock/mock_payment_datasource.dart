import '../../../domain/entities/payment.dart';

/// Mock data source for payment operations
class MockPaymentDataSource {
  /// Get mock dealer plans
  List<DealerPlan> getMockPlans() {
    return [
      const DealerPlan(
        id: 'plan_free',
        type: DealerPlanType.free,
        name: 'Free',
        description: 'Perfect para empezar',
        priceMonthly: 0,
        priceYearly: 0,
        maxListings: 5,
        maxFeaturedListings: 0,
        hasAnalytics: false,
        hasCRM: false,
        hasPrioritySupport: false,
        features: [
          '5 anuncios activos',
          'Estadísticas básicas',
          'Soporte por email',
        ],
        isCurrentPlan: true,
      ),
      const DealerPlan(
        id: 'plan_basic',
        type: DealerPlanType.basic,
        name: 'Basic',
        description: 'Para dealers pequeños',
        priceMonthly: 49.99,
        priceYearly: 499.99,
        maxListings: 25,
        maxFeaturedListings: 3,
        hasAnalytics: true,
        hasCRM: false,
        hasPrioritySupport: false,
        features: [
          '25 anuncios activos',
          '3 anuncios destacados',
          'Analíticas avanzadas',
          'Gestión de leads',
          'Soporte prioritario',
        ],
        isPopular: true,
      ),
      const DealerPlan(
        id: 'plan_pro',
        type: DealerPlanType.pro,
        name: 'Pro',
        description: 'Para dealers profesionales',
        priceMonthly: 149.99,
        priceYearly: 1499.99,
        maxListings: 100,
        maxFeaturedListings: 10,
        hasAnalytics: true,
        hasCRM: true,
        hasPrioritySupport: true,
        features: [
          '100 anuncios activos',
          '10 anuncios destacados',
          'CRM completo',
          'Analíticas premium',
          'API access',
          'Soporte 24/7',
        ],
      ),
      const DealerPlan(
        id: 'plan_enterprise',
        type: DealerPlanType.enterprise,
        name: 'Enterprise',
        description: 'Para grandes concesionarios',
        priceMonthly: 499.99,
        priceYearly: 4999.99,
        maxListings: -1, // Unlimited
        maxFeaturedListings: 50,
        hasAnalytics: true,
        hasCRM: true,
        hasPrioritySupport: true,
        features: [
          'Anuncios ilimitados',
          '50 anuncios destacados',
          'CRM Enterprise',
          'Analíticas personalizadas',
          'API dedicada',
          'Soporte dedicado',
          'Onboarding personalizado',
          'Múltiples ubicaciones',
        ],
      ),
    ];
  }

  /// Get mock current subscription
  Subscription? getMockSubscription() {
    return Subscription(
      id: 'sub_mock123',
      userId: 'user_123',
      plan: getMockPlans()[0], // Free plan
      billingPeriod: BillingPeriod.monthly,
      startDate: DateTime.now().subtract(const Duration(days: 15)),
      nextBillingDate: DateTime.now().add(const Duration(days: 15)),
      isActive: true,
      isCancelled: false,
      usageStats: getMockUsageStats(),
    );
  }

  /// Get mock payment methods
  List<PaymentMethod> getMockPaymentMethods() {
    return [
      const PaymentMethod(
        id: 'pm_visa1234',
        type: 'card',
        brand: 'visa',
        last4: '4242',
        expiryMonth: 12,
        expiryYear: 2025,
        isDefault: true,
      ),
      const PaymentMethod(
        id: 'pm_mastercard5678',
        type: 'card',
        brand: 'mastercard',
        last4: '5555',
        expiryMonth: 6,
        expiryYear: 2026,
        isDefault: false,
      ),
    ];
  }

  /// Get mock payment history
  List<Payment> getMockPaymentHistory() {
    return [
      Payment(
        id: 'pay_001',
        userId: 'user_123',
        subscriptionId: 'sub_mock123',
        amount: 49.99,
        currency: 'USD',
        status: PaymentStatus.completed,
        description: 'Basic Plan - Monthly Subscription',
        paymentMethod: 'card',
        last4: '4242',
        paymentDate: DateTime.now().subtract(const Duration(days: 15)),
        invoiceUrl: 'https://example.com/invoice/inv_001.pdf',
      ),
      Payment(
        id: 'pay_002',
        userId: 'user_123',
        subscriptionId: 'sub_mock123',
        amount: 49.99,
        currency: 'USD',
        status: PaymentStatus.completed,
        description: 'Basic Plan - Monthly Subscription',
        paymentMethod: 'card',
        last4: '4242',
        paymentDate: DateTime.now().subtract(const Duration(days: 45)),
        invoiceUrl: 'https://example.com/invoice/inv_002.pdf',
      ),
      Payment(
        id: 'pay_003',
        userId: 'user_123',
        amount: 29.99,
        currency: 'USD',
        status: PaymentStatus.completed,
        description: 'Featured Listing Boost',
        paymentMethod: 'card',
        last4: '4242',
        paymentDate: DateTime.now().subtract(const Duration(days: 60)),
        invoiceUrl: 'https://example.com/invoice/inv_003.pdf',
      ),
      Payment(
        id: 'pay_004',
        userId: 'user_123',
        subscriptionId: 'sub_mock123',
        amount: 49.99,
        currency: 'USD',
        status: PaymentStatus.failed,
        description: 'Basic Plan - Monthly Subscription',
        paymentMethod: 'card',
        last4: '4242',
        paymentDate: DateTime.now().subtract(const Duration(days: 75)),
      ),
    ];
  }

  /// Get mock usage stats
  UsageStats getMockUsageStats() {
    return const UsageStats(
      currentListings: 8,
      listingsLimit: 25,
      currentFeaturedListings: 2,
      featuredListingsLimit: 3,
    );
  }
}
