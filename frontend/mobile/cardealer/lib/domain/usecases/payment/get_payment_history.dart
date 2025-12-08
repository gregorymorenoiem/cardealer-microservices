import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for getting payment history
class GetPaymentHistoryParams extends Equatable {
  final int? limit;
  final DateTime? startDate;
  final DateTime? endDate;

  const GetPaymentHistoryParams({
    this.limit,
    this.startDate,
    this.endDate,
  });

  @override
  List<Object?> get props => [limit, startDate, endDate];
}

/// Use case for getting payment history
class GetPaymentHistory
    implements UseCase<List<Payment>, GetPaymentHistoryParams> {
  final PaymentRepository repository;

  GetPaymentHistory(this.repository);

  @override
  Future<Either<Failure, List<Payment>>> call(
      GetPaymentHistoryParams params) async {
    return await repository.getPaymentHistory(
      limit: params.limit,
      startDate: params.startDate,
      endDate: params.endDate,
    );
  }
}
