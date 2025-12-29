import 'package:dartz/dartz.dart';
import '../entities/payment.dart';
import '../../core/error/failures.dart';

/// Repository interface for payment operations
abstract class PaymentRepository {
  /// Get all available dealer plans
  Future<Either<Failure, List<DealerPlan>>> getAvailablePlans();

  /// Get current subscription
  Future<Either<Failure, Subscription?>> getCurrentSubscription();

  /// Subscribe to a plan
  Future<Either<Failure, Subscription>> subscribeToPlan({
    required String planId,
    required BillingPeriod billingPeriod,
    required String paymentMethodId,
  });

  /// Update subscription (upgrade/downgrade)
  Future<Either<Failure, Subscription>> updateSubscription({
    required String subscriptionId,
    required String newPlanId,
    required BillingPeriod billingPeriod,
  });

  /// Cancel subscription
  Future<Either<Failure, void>> cancelSubscription(String subscriptionId);

  /// Get payment methods
  Future<Either<Failure, List<PaymentMethod>>> getPaymentMethods();

  /// Add payment method
  Future<Either<Failure, PaymentMethod>> addPaymentMethod({
    required String token,
    required bool setAsDefault,
  });

  /// Remove payment method
  Future<Either<Failure, void>> removePaymentMethod(String paymentMethodId);

  /// Set default payment method
  Future<Either<Failure, void>> setDefaultPaymentMethod(String paymentMethodId);

  /// Get payment history
  Future<Either<Failure, List<Payment>>> getPaymentHistory({
    int? limit,
    DateTime? startDate,
    DateTime? endDate,
  });

  /// Get payment by ID
  Future<Either<Failure, Payment>> getPaymentById(String paymentId);

  /// Process one-time payment
  Future<Either<Failure, Payment>> processPayment({
    required double amount,
    required String paymentMethodId,
    required String description,
  });

  /// Get usage statistics
  Future<Either<Failure, UsageStats>> getUsageStats();

  /// Get invoice by ID
  Future<Either<Failure, String>> getInvoiceUrl(String invoiceId);

  /// Download invoice PDF
  Future<Either<Failure, String>> downloadInvoice(String invoiceId);
}
