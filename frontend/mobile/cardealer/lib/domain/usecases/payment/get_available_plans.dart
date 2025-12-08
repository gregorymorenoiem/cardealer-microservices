import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Use case for getting available dealer plans
class GetAvailablePlans implements UseCase<List<DealerPlan>, NoParams> {
  final PaymentRepository repository;

  GetAvailablePlans(this.repository);

  @override
  Future<Either<Failure, List<DealerPlan>>> call(NoParams params) async {
    return await repository.getAvailablePlans();
  }
}
