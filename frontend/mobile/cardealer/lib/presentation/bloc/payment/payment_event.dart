part of 'payment_bloc.dart';

/// Base class for all payment events
abstract class PaymentEvent extends Equatable {
  const PaymentEvent();

  @override
  List<Object?> get props => [];
}

/// Event to load available plans
class LoadAvailablePlansEvent extends PaymentEvent {
  const LoadAvailablePlansEvent();
}

/// Event to load current subscription
class LoadCurrentSubscriptionEvent extends PaymentEvent {
  const LoadCurrentSubscriptionEvent();
}

/// Event to subscribe to a plan
class SubscribeToPlanEvent extends PaymentEvent {
  final String planId;
  final BillingPeriod billingPeriod;
  final String paymentMethodId;

  const SubscribeToPlanEvent({
    required this.planId,
    required this.billingPeriod,
    required this.paymentMethodId,
  });

  @override
  List<Object?> get props => [planId, billingPeriod, paymentMethodId];
}

/// Event to update subscription
class UpdateSubscriptionEvent extends PaymentEvent {
  final String subscriptionId;
  final String newPlanId;
  final BillingPeriod billingPeriod;

  const UpdateSubscriptionEvent({
    required this.subscriptionId,
    required this.newPlanId,
    required this.billingPeriod,
  });

  @override
  List<Object?> get props => [subscriptionId, newPlanId, billingPeriod];
}

/// Event to cancel subscription
class CancelSubscriptionEvent extends PaymentEvent {
  final String subscriptionId;

  const CancelSubscriptionEvent(this.subscriptionId);

  @override
  List<Object?> get props => [subscriptionId];
}

/// Event to load payment methods
class LoadPaymentMethodsEvent extends PaymentEvent {
  const LoadPaymentMethodsEvent();
}

/// Event to add payment method
class AddPaymentMethodEvent extends PaymentEvent {
  final String token;
  final bool setAsDefault;

  const AddPaymentMethodEvent({
    required this.token,
    this.setAsDefault = false,
  });

  @override
  List<Object?> get props => [token, setAsDefault];
}

/// Event to remove payment method
class RemovePaymentMethodEvent extends PaymentEvent {
  final String paymentMethodId;

  const RemovePaymentMethodEvent(this.paymentMethodId);

  @override
  List<Object?> get props => [paymentMethodId];
}

/// Event to set default payment method
class SetDefaultPaymentMethodEvent extends PaymentEvent {
  final String paymentMethodId;

  const SetDefaultPaymentMethodEvent(this.paymentMethodId);

  @override
  List<Object?> get props => [paymentMethodId];
}

/// Event to load payment history
class LoadPaymentHistoryEvent extends PaymentEvent {
  final int? limit;
  final DateTime? startDate;
  final DateTime? endDate;

  const LoadPaymentHistoryEvent({
    this.limit,
    this.startDate,
    this.endDate,
  });

  @override
  List<Object?> get props => [limit, startDate, endDate];
}

/// Event to load usage stats
class LoadUsageStatsEvent extends PaymentEvent {
  const LoadUsageStatsEvent();
}

/// Event to process one-time payment
class ProcessPaymentEvent extends PaymentEvent {
  final double amount;
  final String paymentMethodId;
  final String description;

  const ProcessPaymentEvent({
    required this.amount,
    required this.paymentMethodId,
    required this.description,
  });

  @override
  List<Object?> get props => [amount, paymentMethodId, description];
}

/// Event to get invoice URL
class GetInvoiceUrlEvent extends PaymentEvent {
  final String invoiceId;

  const GetInvoiceUrlEvent(this.invoiceId);

  @override
  List<Object?> get props => [invoiceId];
}

/// Event to change selected billing period
class ChangeBillingPeriodEvent extends PaymentEvent {
  final BillingPeriod billingPeriod;

  const ChangeBillingPeriodEvent(this.billingPeriod);

  @override
  List<Object?> get props => [billingPeriod];
}
