import 'package:equatable/equatable.dart';
import '../../../domain/entities/vehicle.dart';

/// Base state for favorites
abstract class FavoritesState extends Equatable {
  const FavoritesState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class FavoritesInitial extends FavoritesState {}

/// Loading favorites
class FavoritesLoading extends FavoritesState {}

/// Favorites loaded successfully
class FavoritesLoaded extends FavoritesState {
  final List<Vehicle> favorites;

  const FavoritesLoaded(this.favorites);

  @override
  List<Object?> get props => [favorites];
}

/// Empty favorites state
class FavoritesEmpty extends FavoritesState {}

/// Error state
class FavoritesError extends FavoritesState {
  final String message;

  const FavoritesError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Removing favorite state
class FavoriteRemoving extends FavoritesState {
  final List<Vehicle> currentFavorites;
  final String removingId;

  const FavoriteRemoving(this.currentFavorites, this.removingId);

  @override
  List<Object?> get props => [currentFavorites, removingId];
}

/// Favorite removed successfully
class FavoriteRemoved extends FavoritesState {
  final List<Vehicle> favorites;

  const FavoriteRemoved(this.favorites);

  @override
  List<Object?> get props => [favorites];
}
