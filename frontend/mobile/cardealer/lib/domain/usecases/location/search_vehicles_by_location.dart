import 'package:dartz/dartz.dart';
import 'package:equatable/equatable.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/location.dart';
import '../../repositories/location_repository.dart';

class SearchVehiclesByLocation
    implements UseCase<List<VehicleMarker>, SearchVehiclesByLocationParams> {
  final LocationRepository repository;

  SearchVehiclesByLocation(this.repository);

  @override
  Future<Either<Failure, List<VehicleMarker>>> call(
    SearchVehiclesByLocationParams params,
  ) async {
    return await repository.getVehicleMarkers(
      searchArea: params.searchArea,
      brands: params.brands,
      minPrice: params.minPrice,
      maxPrice: params.maxPrice,
    );
  }
}

class SearchVehiclesByLocationParams extends Equatable {
  final SearchArea searchArea;
  final List<String>? brands;
  final double? minPrice;
  final double? maxPrice;

  const SearchVehiclesByLocationParams({
    required this.searchArea,
    this.brands,
    this.minPrice,
    this.maxPrice,
  });

  @override
  List<Object?> get props => [searchArea, brands, minPrice, maxPrice];
}
