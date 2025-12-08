part of 'payment_bloc.dart';

/// Base class for all payment states
abstract class PaymentState extends Equatable {
  const PaymentState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class PaymentInitial extends PaymentState {}

/// Loading state
class PaymentLoading extends PaymentState {}

/// Plans loaded state
class PlansLoaded extends PaymentState {
  final List<DealerPlan> plans;
  final BillingPeriod selectedPeriod;

  const PlansLoaded({
    required this.plans,
    this.selectedPeriod = BillingPeriod.monthly,
  });

  @override
  List<Object?> get props => [plans, selectedPeriod];

  PlansLoaded copyWith({
    List<DealerPlan>? plans,
    BillingPeriod? selectedPeriod,
  }) {
    return PlansLoaded(
      plans: plans ?? this.plans,
      selectedPeriod: selectedPeriod ?? this.selectedPeriod,
    );
  }
}

/// Subscription loaded state
class SubscriptionLoaded extends PaymentState {
  final Subscription? subscription;
  final UsageStats? usageStats;

  const SubscriptionLoaded({
    this.subscription,
    this.usageStats,
  });

  @override
  List<Object?> get props => [subscription, usageStats];
}

/// Payment methods loaded state
class PaymentMethodsLoaded extends PaymentState {
  final List<PaymentMethod> paymentMethods;

  const PaymentMethodsLoaded(this.paymentMethods);

  @override
  List<Object?> get props => [paymentMethods];
}

/// Payment history loaded state
class PaymentHistoryLoaded extends PaymentState {
  final List<Payment> payments;

  const PaymentHistoryLoaded(this.payments);

  @override
  List<Object?> get props => [payments];
}

/// Payment processing state
class PaymentProcessing extends PaymentState {
  final String message;

  const PaymentProcessing(this.message);

  @override
  List<Object?> get props => [message];
}

/// Payment success state
class PaymentSuccess extends PaymentState {
  final String message;
  final dynamic data;

  const PaymentSuccess(this.message, {this.data});

  @override
  List<Object?> get props => [message, data];
}

/// Payment error state
class PaymentError extends PaymentState {
  final String message;

  const PaymentError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Invoice URL loaded state
class InvoiceUrlLoaded extends PaymentState {
  final String url;

  const InvoiceUrlLoaded(this.url);

  @override
  List<Object?> get props => [url];
}

/// Combined state for dashboard
class PaymentDashboardLoaded extends PaymentState {
  final Subscription? subscription;
  final List<DealerPlan> plans;
  final UsageStats usageStats;
  final List<Payment> recentPayments;

  const PaymentDashboardLoaded({
    required this.plans,
    required this.usageStats,
    required this.recentPayments,
    this.subscription,
  });

  @override
  List<Object?> get props => [subscription, plans, usageStats, recentPayments];
}
