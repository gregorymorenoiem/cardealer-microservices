import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Use case for getting payment methods
class GetPaymentMethods implements UseCase<List<PaymentMethod>, NoParams> {
  final PaymentRepository repository;

  GetPaymentMethods(this.repository);

  @override
  Future<Either<Failure, List<PaymentMethod>>> call(NoParams params) async {
    return await repository.getPaymentMethods();
  }
}
