import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import 'injection.config.dart';
import '../../data/datasources/mock/mock_vehicle_datasource.dart';
import '../../data/repositories/vehicle_repository_impl.dart';
import '../../domain/repositories/vehicle_repository.dart';
import '../../presentation/bloc/vehicles/vehicles_bloc.dart';
import '../network/network_info.dart';

final getIt = GetIt.instance;

@InjectableInit(
  initializerName: 'init',
  preferRelativeImports: true,
  asExtension: true,
)
Future<void> configureDependencies() async {
  await getIt.init();
  
  // Register network info
  getIt.registerLazySingleton<NetworkInfo>(() => NetworkInfoImpl());
  
  // Register data sources
  getIt.registerLazySingleton<MockVehicleDataSource>(() => MockVehicleDataSource());
  
  // Register repositories
  getIt.registerLazySingleton<VehicleRepository>(
    () => VehicleRepositoryImpl(
      mockDataSource: getIt<MockVehicleDataSource>(),
      networkInfo: getIt<NetworkInfo>(),
    ),
  );
  
  // Register BLoCs
  getIt.registerFactory<VehiclesBloc>(
    () => VehiclesBloc(repository: getIt<VehicleRepository>()),
  );
}
