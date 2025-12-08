import 'package:equatable/equatable.dart';
import '../../../domain/entities/vehicle.dart';

/// Base class for all vehicle states
abstract class VehiclesState extends Equatable {
  @override
  List<Object?> get props => [];
}

/// Initial state
class VehiclesInitial extends VehiclesState {}

/// Loading state
class VehiclesLoading extends VehiclesState {}

/// State when vehicles are loaded successfully
class VehiclesLoaded extends VehiclesState {
  final List<Vehicle> heroCarousel;
  final List<Vehicle> featuredGrid;
  final List<Vehicle> weekFeatured;
  final List<Vehicle> dailyDeals;
  final List<Vehicle> suvsAndTrucks;
  final List<Vehicle> premium;
  final List<Vehicle> electricAndHybrid;

  VehiclesLoaded({
    this.heroCarousel = const [],
    this.featuredGrid = const [],
    this.weekFeatured = const [],
    this.dailyDeals = const [],
    this.suvsAndTrucks = const [],
    this.premium = const [],
    this.electricAndHybrid = const [],
  });

  VehiclesLoaded copyWith({
    List<Vehicle>? heroCarousel,
    List<Vehicle>? featuredGrid,
    List<Vehicle>? weekFeatured,
    List<Vehicle>? dailyDeals,
    List<Vehicle>? suvsAndTrucks,
    List<Vehicle>? premium,
    List<Vehicle>? electricAndHybrid,
  }) {
    return VehiclesLoaded(
      heroCarousel: heroCarousel ?? this.heroCarousel,
      featuredGrid: featuredGrid ?? this.featuredGrid,
      weekFeatured: weekFeatured ?? this.weekFeatured,
      dailyDeals: dailyDeals ?? this.dailyDeals,
      suvsAndTrucks: suvsAndTrucks ?? this.suvsAndTrucks,
      premium: premium ?? this.premium,
      electricAndHybrid: electricAndHybrid ?? this.electricAndHybrid,
    );
  }

  @override
  List<Object?> get props => [
        heroCarousel,
        featuredGrid,
        weekFeatured,
        dailyDeals,
        suvsAndTrucks,
        premium,
        electricAndHybrid,
      ];
}

/// State for vehicle search results
class VehiclesSearchResults extends VehiclesState {
  final List<Vehicle> vehicles;

  VehiclesSearchResults(this.vehicles);

  @override
  List<Object?> get props => [vehicles];
}

/// State for single vehicle detail
class VehicleDetailLoaded extends VehiclesState {
  final Vehicle vehicle;

  VehicleDetailLoaded(this.vehicle);

  @override
  List<Object?> get props => [vehicle];
}

/// Error state
class VehiclesError extends VehiclesState {
  final String message;

  VehiclesError(this.message);

  @override
  List<Object?> get props => [message];
}
