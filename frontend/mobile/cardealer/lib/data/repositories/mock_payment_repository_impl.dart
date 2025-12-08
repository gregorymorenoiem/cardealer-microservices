import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../../domain/entities/payment.dart';
import '../../domain/repositories/payment_repository.dart';
import '../datasources/mock/mock_payment_datasource.dart';

/// Mock implementation of PaymentRepository
class MockPaymentRepositoryImpl implements PaymentRepository {
  final MockPaymentDataSource dataSource;

  MockPaymentRepositoryImpl({required this.dataSource});

  @override
  Future<Either<Failure, List<DealerPlan>>> getAvailablePlans() async {
    try {
      await Future.delayed(const Duration(milliseconds: 500));
      final plans = dataSource.getMockPlans();
      return Right(plans);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to load plans'));
    }
  }

  @override
  Future<Either<Failure, Subscription?>> getCurrentSubscription() async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      final subscription = dataSource.getMockSubscription();
      return Right(subscription);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to load subscription'));
    }
  }

  @override
  Future<Either<Failure, Subscription>> subscribeToPlan({
    required String planId,
    required BillingPeriod billingPeriod,
    required String paymentMethodId,
  }) async {
    try {
      await Future.delayed(const Duration(seconds: 2));

      final plans = dataSource.getMockPlans();
      final plan = plans.firstWhere((p) => p.id == planId);

      final subscription = Subscription(
        id: 'sub_${DateTime.now().millisecondsSinceEpoch}',
        userId: 'user_123',
        plan: plan,
        billingPeriod: billingPeriod,
        startDate: DateTime.now(),
        nextBillingDate: DateTime.now().add(
          billingPeriod == BillingPeriod.monthly
              ? const Duration(days: 30)
              : const Duration(days: 365),
        ),
        isActive: true,
        isCancelled: false,
      );

      return Right(subscription);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to subscribe to plan'));
    }
  }

  @override
  Future<Either<Failure, Subscription>> updateSubscription({
    required String subscriptionId,
    required String newPlanId,
    required BillingPeriod billingPeriod,
  }) async {
    try {
      await Future.delayed(const Duration(seconds: 1));

      final plans = dataSource.getMockPlans();
      final plan = plans.firstWhere((p) => p.id == newPlanId);

      final subscription = Subscription(
        id: subscriptionId,
        userId: 'user_123',
        plan: plan,
        billingPeriod: billingPeriod,
        startDate: DateTime.now().subtract(const Duration(days: 15)),
        nextBillingDate: DateTime.now().add(const Duration(days: 15)),
        isActive: true,
        isCancelled: false,
      );

      return Right(subscription);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to update subscription'));
    }
  }

  @override
  Future<Either<Failure, void>> cancelSubscription(
      String subscriptionId) async {
    try {
      await Future.delayed(const Duration(seconds: 1));
      return const Right(null);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to cancel subscription'));
    }
  }

  @override
  Future<Either<Failure, List<PaymentMethod>>> getPaymentMethods() async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      final methods = dataSource.getMockPaymentMethods();
      return Right(methods);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to load payment methods'));
    }
  }

  @override
  Future<Either<Failure, PaymentMethod>> addPaymentMethod({
    required String token,
    required bool setAsDefault,
  }) async {
    try {
      await Future.delayed(const Duration(seconds: 2));

      final method = PaymentMethod(
        id: 'pm_${DateTime.now().millisecondsSinceEpoch}',
        type: 'card',
        brand: 'visa',
        last4:
            '${DateTime.now().millisecondsSinceEpoch % 10000}'.padLeft(4, '0'),
        expiryMonth: 12,
        expiryYear: 2026,
        isDefault: setAsDefault,
      );

      return Right(method);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to add payment method'));
    }
  }

  @override
  Future<Either<Failure, void>> removePaymentMethod(
      String paymentMethodId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 500));
      return const Right(null);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to remove payment method'));
    }
  }

  @override
  Future<Either<Failure, void>> setDefaultPaymentMethod(
      String paymentMethodId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 500));
      return const Right(null);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to set default payment method'));
    }
  }

  @override
  Future<Either<Failure, List<Payment>>> getPaymentHistory({
    int? limit,
    DateTime? startDate,
    DateTime? endDate,
  }) async {
    try {
      await Future.delayed(const Duration(milliseconds: 500));
      var payments = dataSource.getMockPaymentHistory();

      if (startDate != null) {
        payments =
            payments.where((p) => p.paymentDate.isAfter(startDate)).toList();
      }
      if (endDate != null) {
        payments =
            payments.where((p) => p.paymentDate.isBefore(endDate)).toList();
      }
      if (limit != null) {
        payments = payments.take(limit).toList();
      }

      return Right(payments);
    } catch (e) {
      return const Left(
          ServerFailure(message: 'Failed to load payment history'));
    }
  }

  @override
  Future<Either<Failure, Payment>> getPaymentById(String paymentId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      final payments = dataSource.getMockPaymentHistory();
      final payment = payments.firstWhere((p) => p.id == paymentId);
      return Right(payment);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to load payment'));
    }
  }

  @override
  Future<Either<Failure, Payment>> processPayment({
    required double amount,
    required String paymentMethodId,
    required String description,
  }) async {
    try {
      await Future.delayed(const Duration(seconds: 2));

      final payment = Payment(
        id: 'pay_${DateTime.now().millisecondsSinceEpoch}',
        userId: 'user_123',
        amount: amount,
        currency: 'USD',
        status: PaymentStatus.completed,
        paymentMethod: 'card',
        last4: '4242',
        paymentDate: DateTime.now(),
        description: description,
        invoiceUrl:
            'https://example.com/invoice/inv_${DateTime.now().millisecondsSinceEpoch}.pdf',
      );

      return Right(payment);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to process payment'));
    }
  }

  @override
  Future<Either<Failure, UsageStats>> getUsageStats() async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      final stats = dataSource.getMockUsageStats();
      return Right(stats);
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to load usage stats'));
    }
  }

  @override
  Future<Either<Failure, String>> getInvoiceUrl(String invoiceId) async {
    try {
      await Future.delayed(const Duration(milliseconds: 300));
      return Right('https://example.com/invoice/$invoiceId.pdf');
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to get invoice URL'));
    }
  }

  @override
  Future<Either<Failure, String>> downloadInvoice(String invoiceId) async {
    try {
      await Future.delayed(const Duration(seconds: 1));
      return Right('/downloads/invoice_$invoiceId.pdf');
    } catch (e) {
      return const Left(ServerFailure(message: 'Failed to download invoice'));
    }
  }
}
