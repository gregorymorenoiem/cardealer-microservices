import 'package:equatable/equatable.dart';

/// Events for vehicle detail page
abstract class VehicleDetailEvent extends Equatable {
  const VehicleDetailEvent();

  @override
  List<Object?> get props => [];
}

/// Load vehicle details
class LoadVehicleDetail extends VehicleDetailEvent {
  final String vehicleId;

  const LoadVehicleDetail(this.vehicleId);

  @override
  List<Object?> get props => [vehicleId];
}

/// Toggle favorite status
class ToggleVehicleFavorite extends VehicleDetailEvent {
  final String vehicleId;

  const ToggleVehicleFavorite(this.vehicleId);

  @override
  List<Object?> get props => [vehicleId];
}

/// Contact seller
class ContactSellerEvent extends VehicleDetailEvent {
  final String vehicleId;
  final String sellerId;
  final String message;

  const ContactSellerEvent({
    required this.vehicleId,
    required this.sellerId,
    required this.message,
  });

  @override
  List<Object?> get props => [vehicleId, sellerId, message];
}

/// Load similar vehicles
class LoadSimilarVehicles extends VehicleDetailEvent {
  final String currentVehicleId;
  final String? make;
  final String? model;
  final double? priceMin;
  final double? priceMax;

  const LoadSimilarVehicles({
    required this.currentVehicleId,
    this.make,
    this.model,
    this.priceMin,
    this.priceMax,
  });

  @override
  List<Object?> get props =>
      [currentVehicleId, make, model, priceMin, priceMax];
}

/// Share vehicle
class ShareVehicle extends VehicleDetailEvent {
  final String vehicleId;
  final String title;

  const ShareVehicle({
    required this.vehicleId,
    required this.title,
  });

  @override
  List<Object?> get props => [vehicleId, title];
}
