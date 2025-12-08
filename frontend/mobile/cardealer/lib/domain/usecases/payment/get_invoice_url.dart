import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for getting invoice URL
class GetInvoiceUrlParams extends Equatable {
  final String invoiceId;

  const GetInvoiceUrlParams({required this.invoiceId});

  @override
  List<Object?> get props => [invoiceId];
}

/// Use case for getting invoice URL
class GetInvoiceUrl implements UseCase<String, GetInvoiceUrlParams> {
  final PaymentRepository repository;

  GetInvoiceUrl(this.repository);

  @override
  Future<Either<Failure, String>> call(GetInvoiceUrlParams params) async {
    return await repository.getInvoiceUrl(params.invoiceId);
  }
}
