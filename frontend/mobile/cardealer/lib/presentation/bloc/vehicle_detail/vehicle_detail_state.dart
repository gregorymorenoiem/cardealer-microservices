import 'package:equatable/equatable.dart';
import '../../../domain/entities/vehicle.dart';

/// States for vehicle detail page
abstract class VehicleDetailState extends Equatable {
  const VehicleDetailState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class VehicleDetailInitial extends VehicleDetailState {}

/// Loading vehicle details
class VehicleDetailLoading extends VehicleDetailState {}

/// Vehicle details loaded successfully
class VehicleDetailLoaded extends VehicleDetailState {
  final Vehicle vehicle;
  final bool isFavorite;
  final List<Vehicle>? similarVehicles;
  final bool isLoadingSimilar;

  const VehicleDetailLoaded({
    required this.vehicle,
    this.isFavorite = false,
    this.similarVehicles,
    this.isLoadingSimilar = false,
  });

  @override
  List<Object?> get props => [vehicle, isFavorite, similarVehicles, isLoadingSimilar];

  VehicleDetailLoaded copyWith({
    Vehicle? vehicle,
    bool? isFavorite,
    List<Vehicle>? similarVehicles,
    bool? isLoadingSimilar,
  }) {
    return VehicleDetailLoaded(
      vehicle: vehicle ?? this.vehicle,
      isFavorite: isFavorite ?? this.isFavorite,
      similarVehicles: similarVehicles ?? this.similarVehicles,
      isLoadingSimilar: isLoadingSimilar ?? this.isLoadingSimilar,
    );
  }
}

/// Error loading details
class VehicleDetailError extends VehicleDetailState {
  final String message;

  const VehicleDetailError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Contact seller in progress
class ContactingSellerState extends VehicleDetailState {
  final Vehicle vehicle;

  const ContactingSellerState(this.vehicle);

  @override
  List<Object?> get props => [vehicle];
}

/// Contact seller success
class ContactSellerSuccess extends VehicleDetailState {
  final Vehicle vehicle;
  final String conversationId;

  const ContactSellerSuccess({
    required this.vehicle,
    required this.conversationId,
  });

  @override
  List<Object?> get props => [vehicle, conversationId];
}

/// Contact seller error
class ContactSellerError extends VehicleDetailState {
  final Vehicle vehicle;
  final String message;

  const ContactSellerError({
    required this.vehicle,
    required this.message,
  });

  @override
  List<Object?> get props => [vehicle, message];
}

/// Share vehicle success
class ShareVehicleSuccess extends VehicleDetailState {
  final Vehicle vehicle;

  const ShareVehicleSuccess(this.vehicle);

  @override
  List<Object?> get props => [vehicle];
}
