import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/lead.dart';
import '../../repositories/dealer_repository.dart';

class GetLeads implements UseCase<List<Lead>, GetLeadsParams> {
  final DealerRepository repository;

  GetLeads(this.repository);

  @override
  Future<Either<Failure, List<Lead>>> call(GetLeadsParams params) async {
    return await repository.getLeads(
      dealerId: params.dealerId,
      status: params.status,
      priority: params.priority,
      searchQuery: params.searchQuery,
      limit: params.limit,
      offset: params.offset,
    );
  }
}

class GetLeadsParams extends Equatable {
  final String dealerId;
  final LeadStatus? status;
  final LeadPriority? priority;
  final String? searchQuery;
  final int? limit;
  final int? offset;

  const GetLeadsParams({
    required this.dealerId,
    this.status,
    this.priority,
    this.searchQuery,
    this.limit,
    this.offset,
  });

  @override
  List<Object?> get props => [
        dealerId,
        status,
        priority,
        searchQuery,
        limit,
        offset,
      ];
}
