import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'injection.config.dart';
import '../../data/datasources/mock/mock_vehicle_datasource.dart';
import '../../data/repositories/vehicle_repository_impl.dart';
import '../../domain/repositories/vehicle_repository.dart';
import '../../domain/usecases/vehicles/search_vehicles.dart';
import '../../domain/usecases/vehicles/filter_vehicles.dart';
import '../../domain/usecases/vehicles/get_filter_suggestions.dart';
import '../../presentation/bloc/vehicles/vehicles_bloc.dart';
import '../../presentation/bloc/filter/filter_bloc.dart';
import '../../presentation/bloc/search/search_bloc.dart';
import '../network/network_info.dart';

final getIt = GetIt.instance;

@InjectableInit(
  initializerName: 'init',
  preferRelativeImports: true,
  asExtension: true,
)
Future<void> configureDependencies() async {
  await getIt.init();

  // Register SharedPreferences
  final sharedPreferences = await SharedPreferences.getInstance();
  getIt.registerSingleton<SharedPreferences>(sharedPreferences);

  // Register network info
  getIt.registerLazySingleton<NetworkInfo>(() => NetworkInfoImpl());

  // Register data sources
  getIt.registerLazySingleton<MockVehicleDataSource>(
      () => MockVehicleDataSource());

  // Register repositories
  getIt.registerLazySingleton<VehicleRepository>(
    () => VehicleRepositoryImpl(
      mockDataSource: getIt<MockVehicleDataSource>(),
      networkInfo: getIt<NetworkInfo>(),
    ),
  );

  // Register use cases
  getIt.registerLazySingleton<SearchVehicles>(
    () => SearchVehicles(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<FilterVehicles>(
    () => FilterVehicles(getIt<VehicleRepository>()),
  );
  getIt.registerLazySingleton<GetFilterSuggestions>(
    () => GetFilterSuggestions(getIt<VehicleRepository>()),
  );

  // Register BLoCs
  getIt.registerFactory<VehiclesBloc>(
    () => VehiclesBloc(repository: getIt<VehicleRepository>()),
  );
  getIt.registerFactory<FilterBloc>(
    () => FilterBloc(
      filterVehicles: getIt<FilterVehicles>(),
      getFilterSuggestions: getIt<GetFilterSuggestions>(),
    ),
  );
  getIt.registerFactory<SearchBloc>(
    () => SearchBloc(
      searchVehicles: getIt<SearchVehicles>(),
      sharedPreferences: getIt<SharedPreferences>(),
    ),
  );
}
