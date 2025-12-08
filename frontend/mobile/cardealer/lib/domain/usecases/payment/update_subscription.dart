import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for updating subscription
class UpdateSubscriptionParams extends Equatable {
  final String subscriptionId;
  final String newPlanId;
  final BillingPeriod billingPeriod;

  const UpdateSubscriptionParams({
    required this.subscriptionId,
    required this.newPlanId,
    required this.billingPeriod,
  });

  @override
  List<Object?> get props => [subscriptionId, newPlanId, billingPeriod];
}

/// Use case for updating subscription
class UpdateSubscription
    implements UseCase<Subscription, UpdateSubscriptionParams> {
  final PaymentRepository repository;

  UpdateSubscription(this.repository);

  @override
  Future<Either<Failure, Subscription>> call(
      UpdateSubscriptionParams params) async {
    return await repository.updateSubscription(
      subscriptionId: params.subscriptionId,
      newPlanId: params.newPlanId,
      billingPeriod: params.billingPeriod,
    );
  }
}
