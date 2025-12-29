import 'package:equatable/equatable.dart';

/// Base class for all vehicle events
abstract class VehiclesEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

/// Event to load hero carousel vehicles
class LoadHeroCarouselEvent extends VehiclesEvent {}

/// Event to load featured grid vehicles
class LoadFeaturedGridEvent extends VehiclesEvent {}

/// Event to load week's featured vehicles
class LoadWeekFeaturedEvent extends VehiclesEvent {}

/// Event to load daily deals
class LoadDailyDealsEvent extends VehiclesEvent {}

/// Event to load SUVs and trucks
class LoadSUVsAndTrucksEvent extends VehiclesEvent {}

/// Event to load premium vehicles
class LoadPremiumVehiclesEvent extends VehiclesEvent {}

/// Event to load electric and hybrid vehicles
class LoadElectricAndHybridEvent extends VehiclesEvent {}

/// Event to load all vehicles
class LoadAllVehiclesEvent extends VehiclesEvent {}

/// Event to search vehicles
class SearchVehiclesEvent extends VehiclesEvent {
  final String? make;
  final String? model;
  final double? minPrice;
  final double? maxPrice;
  final String? bodyType;
  final String? fuelType;
  final String? condition;

  SearchVehiclesEvent({
    this.make,
    this.model,
    this.minPrice,
    this.maxPrice,
    this.bodyType,
    this.fuelType,
    this.condition,
  });

  @override
  List<Object?> get props => [
        make,
        model,
        minPrice,
        maxPrice,
        bodyType,
        fuelType,
        condition,
      ];
}

/// Event to load vehicle by ID
class LoadVehicleByIdEvent extends VehiclesEvent {
  final String vehicleId;

  LoadVehicleByIdEvent(this.vehicleId);

  @override
  List<Object?> get props => [vehicleId];
}

/// Event to refresh all sections
class RefreshAllSectionsEvent extends VehiclesEvent {}
