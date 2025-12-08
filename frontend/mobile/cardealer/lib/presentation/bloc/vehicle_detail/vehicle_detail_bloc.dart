import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'vehicle_detail_event.dart';
import 'vehicle_detail_state.dart';
import '../../../domain/usecases/vehicles/get_vehicle_detail.dart';
import '../../../domain/usecases/vehicles/contact_seller.dart';
import '../../../domain/usecases/vehicles/get_similar_vehicles.dart';
import '../../../domain/usecases/favorites/toggle_favorite.dart';

/// BLoC for managing vehicle detail page state
class VehicleDetailBloc extends Bloc<VehicleDetailEvent, VehicleDetailState> {
  final GetVehicleDetail getVehicleDetail;
  final ContactSeller contactSeller;
  final GetSimilarVehicles getSimilarVehicles;
  final ToggleFavorite toggleFavorite;
  final SharedPreferences sharedPreferences;

  static const String _favoritesKey = 'favorite_vehicles';

  VehicleDetailBloc({
    required this.getVehicleDetail,
    required this.contactSeller,
    required this.getSimilarVehicles,
    required this.toggleFavorite,
    required this.sharedPreferences,
  }) : super(VehicleDetailInitial()) {
    on<LoadVehicleDetail>(_onLoadVehicleDetail);
    on<ToggleVehicleFavorite>(_onToggleFavorite);
    on<ContactSellerEvent>(_onContactSeller);
    on<LoadSimilarVehicles>(_onLoadSimilarVehicles);
    on<ShareVehicle>(_onShareVehicle);
  }

  Future<void> _onLoadVehicleDetail(
    LoadVehicleDetail event,
    Emitter<VehicleDetailState> emit,
  ) async {
    emit(VehicleDetailLoading());

    final result = await getVehicleDetail(event.vehicleId);

    result.fold(
      (failure) => emit(VehicleDetailError(failure.message)),
      (vehicle) {
        final isFavorite = _isFavorite(vehicle.id);
        emit(VehicleDetailLoaded(
          vehicle: vehicle,
          isFavorite: isFavorite,
        ));

        // Auto-load similar vehicles
        add(LoadSimilarVehicles(
          currentVehicleId: vehicle.id,
          make: vehicle.make,
          model: vehicle.model,
          priceMin: vehicle.price * 0.8, // 20% lower
          priceMax: vehicle.price * 1.2, // 20% higher
        ));
      },
    );
  }

  Future<void> _onToggleFavorite(
    ToggleVehicleFavorite event,
    Emitter<VehicleDetailState> emit,
  ) async {
    if (state is VehicleDetailLoaded) {
      final currentState = state as VehicleDetailLoaded;

      // Toggle favorite
      final newFavoriteStatus = !currentState.isFavorite;

      // Update SharedPreferences
      final favorites = sharedPreferences.getStringList(_favoritesKey) ?? [];
      if (newFavoriteStatus) {
        if (!favorites.contains(event.vehicleId)) {
          favorites.add(event.vehicleId);
        }
      } else {
        favorites.remove(event.vehicleId);
      }
      await sharedPreferences.setStringList(_favoritesKey, favorites);

      // Emit updated state
      emit(currentState.copyWith(isFavorite: newFavoriteStatus));
    }
  }

  Future<void> _onContactSeller(
    ContactSellerEvent event,
    Emitter<VehicleDetailState> emit,
  ) async {
    if (state is VehicleDetailLoaded) {
      final currentState = state as VehicleDetailLoaded;

      emit(ContactingSellerState(currentState.vehicle));

      final result = await contactSeller(
        vehicleId: event.vehicleId,
        sellerId: event.sellerId,
        message: event.message,
      );

      result.fold(
        (failure) => emit(ContactSellerError(
          vehicle: currentState.vehicle,
          message: failure.message,
        )),
        (conversationId) => emit(ContactSellerSuccess(
          vehicle: currentState.vehicle,
          conversationId: conversationId,
        )),
      );
    }
  }

  Future<void> _onLoadSimilarVehicles(
    LoadSimilarVehicles event,
    Emitter<VehicleDetailState> emit,
  ) async {
    if (state is VehicleDetailLoaded) {
      final currentState = state as VehicleDetailLoaded;

      // Show loading indicator
      emit(currentState.copyWith(isLoadingSimilar: true));

      final result = await getSimilarVehicles(
        currentVehicleId: event.currentVehicleId,
        make: event.make,
        model: event.model,
        priceMin: event.priceMin,
        priceMax: event.priceMax,
        limit: 10,
      );

      result.fold(
        (failure) {
          // Don't show error, just keep empty similar list
          emit(currentState.copyWith(
            similarVehicles: [],
            isLoadingSimilar: false,
          ));
        },
        (vehicles) {
          emit(currentState.copyWith(
            similarVehicles: vehicles,
            isLoadingSimilar: false,
          ));
        },
      );
    }
  }

  Future<void> _onShareVehicle(
    ShareVehicle event,
    Emitter<VehicleDetailState> emit,
  ) async {
    if (state is VehicleDetailLoaded) {
      final currentState = state as VehicleDetailLoaded;

      // TODO: Implement share functionality using share_plus package
      // For now, just emit success
      emit(ShareVehicleSuccess(currentState.vehicle));

      // Return to loaded state after brief delay
      await Future.delayed(const Duration(milliseconds: 500));
      emit(currentState);
    }
  }

  bool _isFavorite(String vehicleId) {
    final favorites = sharedPreferences.getStringList(_favoritesKey) ?? [];
    return favorites.contains(vehicleId);
  }
}
