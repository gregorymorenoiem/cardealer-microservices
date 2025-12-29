import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/usecases/favorites/get_favorites.dart';
import '../../../domain/usecases/favorites/remove_favorite.dart';
import '../../../domain/usecases/favorites/toggle_favorite.dart';
import 'favorites_event.dart';
import 'favorites_state.dart';

/// BLoC for managing favorite vehicles
class FavoritesBloc extends Bloc<FavoritesEvent, FavoritesState> {
  final GetFavorites getFavorites;
  final RemoveFavorite removeFavorite;
  final ToggleFavorite toggleFavorite;

  FavoritesBloc({
    required this.getFavorites,
    required this.removeFavorite,
    required this.toggleFavorite,
  }) : super(FavoritesInitial()) {
    on<LoadFavorites>(_onLoadFavorites);
    on<RemoveFavoriteEvent>(_onRemoveFavorite);
    on<ClearAllFavorites>(_onClearAllFavorites);
    on<ToggleFavoriteEvent>(_onToggleFavorite);
    on<RefreshFavorites>(_onRefreshFavorites);
  }

  Future<void> _onLoadFavorites(
    LoadFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    emit(FavoritesLoading());

    final result = await getFavorites();

    result.fold(
      (failure) => emit(FavoritesError(failure.message)),
      (favorites) {
        if (favorites.isEmpty) {
          emit(FavoritesEmpty());
        } else {
          emit(FavoritesLoaded(favorites));
        }
      },
    );
  }

  Future<void> _onRemoveFavorite(
    RemoveFavoriteEvent event,
    Emitter<FavoritesState> emit,
  ) async {
    if (state is! FavoritesLoaded) return;

    final currentFavorites = (state as FavoritesLoaded).favorites;
    emit(FavoriteRemoving(currentFavorites, event.vehicleId));

    final result = await removeFavorite(event.vehicleId);

    result.fold(
      (failure) => emit(FavoritesError(failure.message)),
      (_) {
        // Reload favorites after removal
        add(LoadFavorites());
      },
    );
  }

  Future<void> _onClearAllFavorites(
    ClearAllFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    if (state is! FavoritesLoaded) return;

    final currentFavorites = (state as FavoritesLoaded).favorites;

    // Remove all favorites one by one
    for (final vehicle in currentFavorites) {
      await removeFavorite(vehicle.id);
    }

    emit(FavoritesEmpty());
  }

  Future<void> _onToggleFavorite(
    ToggleFavoriteEvent event,
    Emitter<FavoritesState> emit,
  ) async {
    final result = await toggleFavorite(event.vehicleId);

    result.fold(
      (failure) => emit(FavoritesError(failure.message)),
      (_) {
        // Reload favorites after toggle
        add(LoadFavorites());
      },
    );
  }

  Future<void> _onRefreshFavorites(
    RefreshFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    final result = await getFavorites();

    result.fold(
      (failure) => emit(FavoritesError(failure.message)),
      (favorites) {
        if (favorites.isEmpty) {
          emit(FavoritesEmpty());
        } else {
          emit(FavoritesLoaded(favorites));
        }
      },
    );
  }
}
