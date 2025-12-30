import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:dartz/dartz.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:cardealer_mobile/domain/repositories/vehicle_repository.dart';
import 'package:cardealer_mobile/domain/entities/vehicle.dart';
import 'package:cardealer_mobile/core/error/failures.dart';
import 'package:cardealer_mobile/presentation/bloc/vehicles/vehicles_bloc.dart';
import 'package:cardealer_mobile/presentation/bloc/vehicles/vehicles_event.dart';
import 'package:cardealer_mobile/presentation/bloc/vehicles/vehicles_state.dart';

// Mock class using mocktail - no code generation needed
class MockVehicleRepository extends Mock implements VehicleRepository {}

void main() {
  late VehiclesBloc vehiclesBloc;
  late MockVehicleRepository mockVehicleRepository;

  setUp(() {
    mockVehicleRepository = MockVehicleRepository();
    vehiclesBloc = VehiclesBloc(
      repository: mockVehicleRepository,
    );
  });

  tearDown(() {
    vehiclesBloc.close();
  });

  // Test data
  final testVehicles = [
    Vehicle(
      id: '1',
      make: 'Toyota',
      model: 'Camry',
      year: 2023,
      price: 35000,
      mileage: 10000,
      condition: 'New',
      transmission: 'Automatic',
      fuelType: 'Gasoline',
      bodyType: 'Sedan',
      images: const ['image1.jpg'],
      description: 'Great car',
      location: 'Miami, FL',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime(2024, 1, 1),
    ),
    Vehicle(
      id: '2',
      make: 'Honda',
      model: 'Accord',
      year: 2022,
      price: 28000,
      mileage: 15000,
      condition: 'Used',
      transmission: 'Automatic',
      fuelType: 'Gasoline',
      bodyType: 'Sedan',
      images: const ['image2.jpg'],
      description: 'Reliable vehicle',
      location: 'Orlando, FL',
      isFeatured: false,
      isVerified: true,
      createdAt: DateTime(2024, 1, 2),
    ),
  ];

  group('VehiclesBloc', () {
    test('initial state should be VehiclesInitial', () {
      expect(vehiclesBloc.state, isA<VehiclesInitial>());
    });

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesLoaded] when LoadHeroCarouselEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getHeroCarouselVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadHeroCarouselEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>().having(
          (state) => state.heroCarousel.length,
          'heroCarousel count',
          2,
        ),
      ],
      verify: (_) {
        verify(() => mockVehicleRepository.getHeroCarouselVehicles()).called(1);
      },
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesError] when LoadHeroCarouselEvent fails',
      build: () {
        when(() => mockVehicleRepository.getHeroCarouselVehicles())
            .thenAnswer((_) async => const Left(ServerFailure(message: 'Network error')));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadHeroCarouselEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesError>().having(
          (state) => state.message,
          'error message',
          'Network error',
        ),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesLoaded] when LoadFeaturedGridEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getFeaturedGridVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadFeaturedGridEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>().having(
          (state) => state.featuredGrid.length,
          'featuredGrid count',
          2,
        ),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesLoaded] when LoadDailyDealsEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getDailyDeals())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadDailyDealsEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>().having(
          (state) => state.dailyDeals.length,
          'dailyDeals count',
          2,
        ),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesLoaded] when LoadPremiumVehiclesEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getPremiumVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadPremiumVehiclesEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>().having(
          (state) => state.premium.length,
          'premium count',
          2,
        ),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesLoaded] when LoadElectricAndHybridEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getElectricAndHybrid())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadElectricAndHybridEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>().having(
          (state) => state.electricAndHybrid.length,
          'electricAndHybrid count',
          2,
        ),
      ],
    );
  });

  group('VehiclesBloc - Search', () {
    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesSearchResults] when SearchVehiclesEvent is successful',
      build: () {
        when(() => mockVehicleRepository.searchVehicles(
          make: any(named: 'make'),
          model: any(named: 'model'),
          minPrice: any(named: 'minPrice'),
          maxPrice: any(named: 'maxPrice'),
          bodyType: any(named: 'bodyType'),
          fuelType: any(named: 'fuelType'),
          condition: any(named: 'condition'),
        )).thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(SearchVehiclesEvent(make: 'Toyota')),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesSearchResults>(),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesError] when SearchVehiclesEvent fails',
      build: () {
        when(() => mockVehicleRepository.searchVehicles(
          make: any(named: 'make'),
          model: any(named: 'model'),
          minPrice: any(named: 'minPrice'),
          maxPrice: any(named: 'maxPrice'),
          bodyType: any(named: 'bodyType'),
          fuelType: any(named: 'fuelType'),
          condition: any(named: 'condition'),
        )).thenAnswer((_) async => const Left(ServerFailure(message: 'Search failed')));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(SearchVehiclesEvent(make: 'Unknown')),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesError>(),
      ],
    );
  });

  group('VehiclesBloc - Load By Id', () {
    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehicleDetailLoaded] when LoadVehicleByIdEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getVehicleById(any()))
            .thenAnswer((_) async => Right(testVehicles.first));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadVehicleByIdEvent('1')),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehicleDetailLoaded>(),
      ],
    );

    blocTest<VehiclesBloc, VehiclesState>(
      'emits [VehiclesLoading, VehiclesError] when LoadVehicleByIdEvent fails',
      build: () {
        when(() => mockVehicleRepository.getVehicleById(any()))
            .thenAnswer((_) async => const Left(ServerFailure(message: 'Vehicle not found')));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(LoadVehicleByIdEvent('invalid')),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesError>(),
      ],
    );
  });

  group('VehiclesBloc - Refresh All', () {
    blocTest<VehiclesBloc, VehiclesState>(
      'emits loading and loaded when RefreshAllSectionsEvent is successful',
      build: () {
        when(() => mockVehicleRepository.getHeroCarouselVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getFeaturedGridVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getWeekFeaturedVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getDailyDeals())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getSUVsAndTrucks())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getPremiumVehicles())
            .thenAnswer((_) async => Right(testVehicles));
        when(() => mockVehicleRepository.getElectricAndHybrid())
            .thenAnswer((_) async => Right(testVehicles));
        return vehiclesBloc;
      },
      act: (bloc) => bloc.add(RefreshAllSectionsEvent()),
      expect: () => [
        isA<VehiclesLoading>(),
        isA<VehiclesLoaded>(),
      ],
    );
  });
}
