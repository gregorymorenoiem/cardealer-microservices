import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/payment.dart';
import '../../repositories/payment_repository.dart';

/// Parameters for adding a payment method
class AddPaymentMethodParams extends Equatable {
  final String token;
  final bool setAsDefault;

  const AddPaymentMethodParams({
    required this.token,
    this.setAsDefault = false,
  });

  @override
  List<Object?> get props => [token, setAsDefault];
}

/// Use case for adding a payment method
class AddPaymentMethod
    implements UseCase<PaymentMethod, AddPaymentMethodParams> {
  final PaymentRepository repository;

  AddPaymentMethod(this.repository);

  @override
  Future<Either<Failure, PaymentMethod>> call(
      AddPaymentMethodParams params) async {
    return await repository.addPaymentMethod(
      token: params.token,
      setAsDefault: params.setAsDefault,
    );
  }
}
