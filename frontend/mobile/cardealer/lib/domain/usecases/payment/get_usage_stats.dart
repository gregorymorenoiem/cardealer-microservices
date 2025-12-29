import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Use case for getting usage statistics
class GetUsageStats implements UseCase<UsageStats, NoParams> {
  final PaymentRepository repository;

  GetUsageStats(this.repository);

  @override
  Future<Either<Failure, UsageStats>> call(NoParams params) async {
    return await repository.getUsageStats();
  }
}
