import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/repositories/vehicle_repository.dart';
import 'vehicles_event.dart';
import 'vehicles_state.dart';

/// BLoC for managing vehicles state
class VehiclesBloc extends Bloc<VehiclesEvent, VehiclesState> {
  final VehicleRepository repository;

  VehiclesBloc({required this.repository}) : super(VehiclesInitial()) {
    on<LoadHeroCarouselEvent>(_onLoadHeroCarousel);
    on<LoadFeaturedGridEvent>(_onLoadFeaturedGrid);
    on<LoadWeekFeaturedEvent>(_onLoadWeekFeatured);
    on<LoadDailyDealsEvent>(_onLoadDailyDeals);
    on<LoadSUVsAndTrucksEvent>(_onLoadSUVsAndTrucks);
    on<LoadPremiumVehiclesEvent>(_onLoadPremiumVehicles);
    on<LoadElectricAndHybridEvent>(_onLoadElectricAndHybrid);
    on<RefreshAllSectionsEvent>(_onRefreshAllSections);
    on<LoadVehicleByIdEvent>(_onLoadVehicleById);
    on<SearchVehiclesEvent>(_onSearchVehicles);
  }

  Future<void> _onLoadHeroCarousel(
    LoadHeroCarouselEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getHeroCarouselVehicles();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(heroCarousel: vehicles));
        } else {
          emit(VehiclesLoaded(heroCarousel: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadFeaturedGrid(
    LoadFeaturedGridEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getFeaturedGridVehicles();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(featuredGrid: vehicles));
        } else {
          emit(VehiclesLoaded(featuredGrid: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadWeekFeatured(
    LoadWeekFeaturedEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getWeekFeaturedVehicles();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(weekFeatured: vehicles));
        } else {
          emit(VehiclesLoaded(weekFeatured: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadDailyDeals(
    LoadDailyDealsEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getDailyDeals();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(dailyDeals: vehicles));
        } else {
          emit(VehiclesLoaded(dailyDeals: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadSUVsAndTrucks(
    LoadSUVsAndTrucksEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getSUVsAndTrucks();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(suvsAndTrucks: vehicles));
        } else {
          emit(VehiclesLoaded(suvsAndTrucks: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadPremiumVehicles(
    LoadPremiumVehiclesEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getPremiumVehicles();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(premium: vehicles));
        } else {
          emit(VehiclesLoaded(premium: vehicles));
        }
      },
    );
  }

  Future<void> _onLoadElectricAndHybrid(
    LoadElectricAndHybridEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    final currentState = state;
    if (currentState is! VehiclesLoaded) {
      emit(VehiclesLoading());
    }

    final result = await repository.getElectricAndHybrid();

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) {
        if (currentState is VehiclesLoaded) {
          emit(currentState.copyWith(electricAndHybrid: vehicles));
        } else {
          emit(VehiclesLoaded(electricAndHybrid: vehicles));
        }
      },
    );
  }

  Future<void> _onRefreshAllSections(
    RefreshAllSectionsEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    emit(VehiclesLoading());

    final results = await Future.wait([
      repository.getHeroCarouselVehicles(),
      repository.getFeaturedGridVehicles(),
      repository.getWeekFeaturedVehicles(),
      repository.getDailyDeals(),
      repository.getSUVsAndTrucks(),
      repository.getPremiumVehicles(),
      repository.getElectricAndHybrid(),
    ]);

    // Check if any failed
    final failures = results.where((r) => r.isLeft()).toList();
    if (failures.isNotEmpty) {
      failures.first.fold(
        (failure) => emit(VehiclesError(failure.message)),
        (_) {},
      );
      return;
    }

    // Extract all successful results
    emit(VehiclesLoaded(
      heroCarousel: results[0].getOrElse(() => []),
      featuredGrid: results[1].getOrElse(() => []),
      weekFeatured: results[2].getOrElse(() => []),
      dailyDeals: results[3].getOrElse(() => []),
      suvsAndTrucks: results[4].getOrElse(() => []),
      premium: results[5].getOrElse(() => []),
      electricAndHybrid: results[6].getOrElse(() => []),
    ));
  }

  Future<void> _onLoadVehicleById(
    LoadVehicleByIdEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    emit(VehiclesLoading());

    final result = await repository.getVehicleById(event.vehicleId);

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicle) => emit(VehicleDetailLoaded(vehicle)),
    );
  }

  Future<void> _onSearchVehicles(
    SearchVehiclesEvent event,
    Emitter<VehiclesState> emit,
  ) async {
    emit(VehiclesLoading());

    final result = await repository.searchVehicles(
      make: event.make,
      model: event.model,
      minPrice: event.minPrice,
      maxPrice: event.maxPrice,
      bodyType: event.bodyType,
      fuelType: event.fuelType,
      condition: event.condition,
    );

    result.fold(
      (failure) => emit(VehiclesError(failure.message)),
      (vehicles) => emit(VehiclesSearchResults(vehicles)),
    );
  }
}
