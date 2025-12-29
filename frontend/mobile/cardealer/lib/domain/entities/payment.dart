import 'package:equatable/equatable.dart';

/// Enum for dealer plan types
enum DealerPlanType {
  free,
  basic,
  pro,
  enterprise,
}

/// Enum for billing period
enum BillingPeriod {
  monthly,
  yearly,
}

/// Enum for payment status
enum PaymentStatus {
  pending,
  completed,
  failed,
  refunded,
}

/// Entity representing a dealer subscription plan
class DealerPlan extends Equatable {
  /// Unique identifier
  final String id;

  /// Plan type
  final DealerPlanType type;

  /// Plan name
  final String name;

  /// Plan description
  final String description;

  /// Price per month
  final double priceMonthly;

  /// Price per year
  final double priceYearly;

  /// Maximum listings allowed
  final int maxListings;

  /// Maximum featured listings allowed
  final int maxFeaturedListings;

  /// Whether analytics are included
  final bool hasAnalytics;

  /// Whether CRM is included
  final bool hasCRM;

  /// Whether priority support is included
  final bool hasPrioritySupport;

  /// Features list
  final List<String> features;

  /// Whether this is the current user's plan
  final bool isCurrentPlan;

  /// Whether this is a popular plan
  final bool isPopular;

  const DealerPlan({
    required this.id,
    required this.type,
    required this.name,
    required this.description,
    required this.priceMonthly,
    required this.priceYearly,
    required this.maxListings,
    required this.maxFeaturedListings,
    required this.hasAnalytics,
    required this.hasCRM,
    required this.hasPrioritySupport,
    required this.features,
    this.isCurrentPlan = false,
    this.isPopular = false,
  });

  /// Get yearly savings percentage
  double get yearlySavingsPercent {
    final yearlyEquivalent = priceMonthly * 12;
    return ((yearlyEquivalent - priceYearly) / yearlyEquivalent) * 100;
  }

  /// Get price for billing period
  double getPriceForPeriod(BillingPeriod period) {
    return period == BillingPeriod.monthly ? priceMonthly : priceYearly;
  }

  /// Get formatted price
  String getFormattedPrice(BillingPeriod period) {
    final price = getPriceForPeriod(period);
    return '\$${price.toStringAsFixed(2)}';
  }

  DealerPlan copyWith({
    String? id,
    DealerPlanType? type,
    String? name,
    String? description,
    double? priceMonthly,
    double? priceYearly,
    int? maxListings,
    int? maxFeaturedListings,
    bool? hasAnalytics,
    bool? hasCRM,
    bool? hasPrioritySupport,
    List<String>? features,
    bool? isCurrentPlan,
    bool? isPopular,
  }) {
    return DealerPlan(
      id: id ?? this.id,
      type: type ?? this.type,
      name: name ?? this.name,
      description: description ?? this.description,
      priceMonthly: priceMonthly ?? this.priceMonthly,
      priceYearly: priceYearly ?? this.priceYearly,
      maxListings: maxListings ?? this.maxListings,
      maxFeaturedListings: maxFeaturedListings ?? this.maxFeaturedListings,
      hasAnalytics: hasAnalytics ?? this.hasAnalytics,
      hasCRM: hasCRM ?? this.hasCRM,
      hasPrioritySupport: hasPrioritySupport ?? this.hasPrioritySupport,
      features: features ?? this.features,
      isCurrentPlan: isCurrentPlan ?? this.isCurrentPlan,
      isPopular: isPopular ?? this.isPopular,
    );
  }

  @override
  List<Object?> get props => [
        id,
        type,
        name,
        description,
        priceMonthly,
        priceYearly,
        maxListings,
        maxFeaturedListings,
        hasAnalytics,
        hasCRM,
        hasPrioritySupport,
        features,
        isCurrentPlan,
        isPopular,
      ];
}

/// Entity representing a subscription
class Subscription extends Equatable {
  /// Unique identifier
  final String id;

  /// User/Dealer ID
  final String userId;

  /// Plan
  final DealerPlan plan;

  /// Billing period
  final BillingPeriod billingPeriod;

  /// Start date
  final DateTime startDate;

  /// End date
  final DateTime? endDate;

  /// Next billing date
  final DateTime? nextBillingDate;

  /// Whether subscription is active
  final bool isActive;

  /// Whether subscription is cancelled
  final bool isCancelled;

  /// Cancellation date
  final DateTime? cancelledAt;

  /// Current period usage stats
  final UsageStats? usageStats;

  const Subscription({
    required this.id,
    required this.userId,
    required this.plan,
    required this.billingPeriod,
    required this.startDate,
    required this.isActive,
    this.endDate,
    this.nextBillingDate,
    this.isCancelled = false,
    this.cancelledAt,
    this.usageStats,
  });

  /// Check if subscription is expiring soon (within 7 days)
  bool get isExpiringSoon {
    if (nextBillingDate == null) return false;
    final daysUntilExpiry = nextBillingDate!.difference(DateTime.now()).inDays;
    return daysUntilExpiry <= 7 && daysUntilExpiry >= 0;
  }

  /// Get days until next billing
  int? get daysUntilNextBilling {
    if (nextBillingDate == null) return null;
    return nextBillingDate!.difference(DateTime.now()).inDays;
  }

  Subscription copyWith({
    String? id,
    String? userId,
    DealerPlan? plan,
    BillingPeriod? billingPeriod,
    DateTime? startDate,
    DateTime? endDate,
    DateTime? nextBillingDate,
    bool? isActive,
    bool? isCancelled,
    DateTime? cancelledAt,
    UsageStats? usageStats,
  }) {
    return Subscription(
      id: id ?? this.id,
      userId: userId ?? this.userId,
      plan: plan ?? this.plan,
      billingPeriod: billingPeriod ?? this.billingPeriod,
      startDate: startDate ?? this.startDate,
      endDate: endDate ?? this.endDate,
      nextBillingDate: nextBillingDate ?? this.nextBillingDate,
      isActive: isActive ?? this.isActive,
      isCancelled: isCancelled ?? this.isCancelled,
      cancelledAt: cancelledAt ?? this.cancelledAt,
      usageStats: usageStats ?? this.usageStats,
    );
  }

  @override
  List<Object?> get props => [
        id,
        userId,
        plan,
        billingPeriod,
        startDate,
        endDate,
        nextBillingDate,
        isActive,
        isCancelled,
        cancelledAt,
        usageStats,
      ];
}

/// Entity representing usage statistics for current period
class UsageStats extends Equatable {
  /// Current listings count
  final int currentListings;

  /// Current featured listings count
  final int currentFeaturedListings;

  /// Listings limit
  final int listingsLimit;

  /// Featured listings limit
  final int featuredListingsLimit;

  const UsageStats({
    required this.currentListings,
    required this.currentFeaturedListings,
    required this.listingsLimit,
    required this.featuredListingsLimit,
  });

  /// Get listings usage percentage
  double get listingsUsagePercent {
    if (listingsLimit == 0) return 0;
    return (currentListings / listingsLimit) * 100;
  }

  /// Get featured listings usage percentage
  double get featuredUsagePercent {
    if (featuredListingsLimit == 0) return 0;
    return (currentFeaturedListings / featuredListingsLimit) * 100;
  }

  /// Check if listings limit reached
  bool get isListingsLimitReached => currentListings >= listingsLimit;

  /// Check if featured limit reached
  bool get isFeaturedLimitReached =>
      currentFeaturedListings >= featuredListingsLimit;

  @override
  List<Object?> get props => [
        currentListings,
        currentFeaturedListings,
        listingsLimit,
        featuredListingsLimit,
      ];
}

/// Entity representing a payment/invoice
class Payment extends Equatable {
  /// Unique identifier
  final String id;

  /// User ID
  final String userId;

  /// Subscription ID
  final String? subscriptionId;

  /// Amount
  final double amount;

  /// Currency
  final String currency;

  /// Payment status
  final PaymentStatus status;

  /// Payment method (e.g., 'card', 'paypal')
  final String paymentMethod;

  /// Last 4 digits of card (if applicable)
  final String? last4;

  /// Payment date
  final DateTime paymentDate;

  /// Description
  final String description;

  /// Invoice URL
  final String? invoiceUrl;

  const Payment({
    required this.id,
    required this.userId,
    required this.amount,
    required this.currency,
    required this.status,
    required this.paymentMethod,
    required this.paymentDate,
    required this.description,
    this.subscriptionId,
    this.last4,
    this.invoiceUrl,
  });

  /// Get formatted amount
  String get formattedAmount {
    return '\$${amount.toStringAsFixed(2)} $currency';
  }

  @override
  List<Object?> get props => [
        id,
        userId,
        subscriptionId,
        amount,
        currency,
        status,
        paymentMethod,
        last4,
        paymentDate,
        description,
        invoiceUrl,
      ];
}

/// Entity representing a payment method
class PaymentMethod extends Equatable {
  /// Unique identifier
  final String id;

  /// Type (e.g., 'card', 'paypal')
  final String type;

  /// Last 4 digits
  final String? last4;

  /// Brand (e.g., 'visa', 'mastercard')
  final String? brand;

  /// Expiry month
  final int? expiryMonth;

  /// Expiry year
  final int? expiryYear;

  /// Whether this is the default payment method
  final bool isDefault;

  const PaymentMethod({
    required this.id,
    required this.type,
    this.last4,
    this.brand,
    this.expiryMonth,
    this.expiryYear,
    this.isDefault = false,
  });

  /// Get display name
  String get displayName {
    if (type == 'card' && brand != null && last4 != null) {
      return '${brand!.toUpperCase()} •••• $last4';
    }
    return type.toUpperCase();
  }

  /// Check if card is expiring soon (within 3 months)
  bool get isExpiringSoon {
    if (expiryMonth == null || expiryYear == null) return false;

    final now = DateTime.now();
    final expiryDate = DateTime(expiryYear!, expiryMonth!);
    final monthsUntilExpiry = expiryDate.difference(now).inDays ~/ 30;

    return monthsUntilExpiry <= 3 && monthsUntilExpiry >= 0;
  }

  @override
  List<Object?> get props => [
        id,
        type,
        last4,
        brand,
        expiryMonth,
        expiryYear,
        isDefault,
      ];
}
