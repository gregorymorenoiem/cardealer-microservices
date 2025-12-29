import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for subscribing to a plan
class SubscribeToPlanParams extends Equatable {
  final String planId;
  final BillingPeriod billingPeriod;
  final String paymentMethodId;

  const SubscribeToPlanParams({
    required this.planId,
    required this.billingPeriod,
    required this.paymentMethodId,
  });

  @override
  List<Object?> get props => [planId, billingPeriod, paymentMethodId];
}

/// Use case for subscribing to a plan
class SubscribeToPlan implements UseCase<Subscription, SubscribeToPlanParams> {
  final PaymentRepository repository;

  SubscribeToPlan(this.repository);

  @override
  Future<Either<Failure, Subscription>> call(
      SubscribeToPlanParams params) async {
    return await repository.subscribeToPlan(
      planId: params.planId,
      billingPeriod: params.billingPeriod,
      paymentMethodId: params.paymentMethodId,
    );
  }
}
