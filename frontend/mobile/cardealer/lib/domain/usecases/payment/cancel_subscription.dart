import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for canceling subscription
class CancelSubscriptionParams extends Equatable {
  final String subscriptionId;

  const CancelSubscriptionParams({required this.subscriptionId});

  @override
  List<Object?> get props => [subscriptionId];
}

/// Use case for canceling subscription
class CancelSubscription implements UseCase<void, CancelSubscriptionParams> {
  final PaymentRepository repository;

  CancelSubscription(this.repository);

  @override
  Future<Either<Failure, void>> call(CancelSubscriptionParams params) async {
    return await repository.cancelSubscription(params.subscriptionId);
  }
}
