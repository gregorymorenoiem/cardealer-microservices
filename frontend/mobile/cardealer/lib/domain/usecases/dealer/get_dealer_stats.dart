import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/dealer_stats.dart';
import '../../repositories/dealer_repository.dart';

class GetDealerStats implements UseCase<DealerStats, String> {
  final DealerRepository repository;

  GetDealerStats(this.repository);

  @override
  Future<Either<Failure, DealerStats>> call(String dealerId) async {
    return await repository.getDealerStats(dealerId);
  }
}
