import 'package:equatable/equatable.dart';

/// Base event for favorites
abstract class FavoritesEvent extends Equatable {
  const FavoritesEvent();

  @override
  List<Object?> get props => [];
}

/// Load all favorites
class LoadFavorites extends FavoritesEvent {}

/// Remove a vehicle from favorites
class RemoveFavoriteEvent extends FavoritesEvent {
  final String vehicleId;

  const RemoveFavoriteEvent(this.vehicleId);

  @override
  List<Object?> get props => [vehicleId];
}

/// Clear all favorites
class ClearAllFavorites extends FavoritesEvent {}

/// Toggle favorite status
class ToggleFavoriteEvent extends FavoritesEvent {
  final String vehicleId;

  const ToggleFavoriteEvent(this.vehicleId);

  @override
  List<Object?> get props => [vehicleId];
}

/// Refresh favorites list
class RefreshFavorites extends FavoritesEvent {}
