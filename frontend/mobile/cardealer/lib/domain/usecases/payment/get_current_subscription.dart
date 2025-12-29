import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Use case for getting current subscription
class GetCurrentSubscription implements UseCase<Subscription?, NoParams> {
  final PaymentRepository repository;

  GetCurrentSubscription(this.repository);

  @override
  Future<Either<Failure, Subscription?>> call(NoParams params) async {
    return await repository.getCurrentSubscription();
  }
}
