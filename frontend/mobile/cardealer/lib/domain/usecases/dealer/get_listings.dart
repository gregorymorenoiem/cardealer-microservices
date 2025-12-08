import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/dealer_listing.dart';
import '../../repositories/dealer_repository.dart';

class GetListings implements UseCase<List<DealerListing>, GetListingsParams> {
  final DealerRepository repository;

  GetListings(this.repository);

  @override
  Future<Either<Failure, List<DealerListing>>> call(
    GetListingsParams params,
  ) async {
    return await repository.getListings(
      dealerId: params.dealerId,
      status: params.status,
      searchQuery: params.searchQuery,
      limit: params.limit,
      offset: params.offset,
    );
  }
}

class GetListingsParams extends Equatable {
  final String dealerId;
  final ListingStatus? status;
  final String? searchQuery;
  final int? limit;
  final int? offset;

  const GetListingsParams({
    required this.dealerId,
    this.status,
    this.searchQuery,
    this.limit,
    this.offset,
  });

  @override
  List<Object?> get props => [dealerId, status, searchQuery, limit, offset];
}
