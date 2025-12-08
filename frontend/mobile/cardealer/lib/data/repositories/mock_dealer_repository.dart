import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../../domain/entities/dealer_listing.dart';
import '../../domain/entities/dealer_stats.dart';
import '../../domain/entities/lead.dart';
import '../../domain/repositories/dealer_repository.dart';
import '../models/dealer_listing_model.dart';
import '../models/dealer_stats_model.dart';
import '../models/lead_model.dart';

class MockDealerRepository implements DealerRepository {
  final List<DealerListingModel> _listings = [];
  final List<LeadModel> _leads = [];
  int _listingIdCounter = 1;
  int _leadIdCounter = 1;

  MockDealerRepository() {
    _initializeMockData();
  }

  void _initializeMockData() {
    // Create 5 mock listings
    final now = DateTime.now();

    _listings.addAll([
      DealerListingModel(
        id: 'listing-${_listingIdCounter++}',
        dealerId: 'dealer-1',
        vehicleId: 'vehicle-1',
        title: 'Toyota Corolla 2022 - Como Nuevo',
        description:
            'Excelente Toyota Corolla 2022 en perfectas condiciones. Unico dueño, mantenimiento al día en agencia oficial. Ideal para quien busca confiabilidad y economía.',
        images: const [
          'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb',
          'https://images.unsplash.com/photo-1619405399517-d7fce0f13302',
        ],
        videoUrl: null,
        brand: 'Toyota',
        model: 'Corolla',
        year: 2022,
        color: 'Blanco',
        mileage: 25000,
        condition: VehicleCondition.usado,
        vin: 'JTDKARFU2K3000001',
        price: 18500,
        discountPrice: 17800,
        currency: 'USD',
        negotiable: true,
        transmission: 'Automática',
        fuelType: 'Gasolina',
        engineSize: '1.8L',
        doors: 4,
        seats: 5,
        features: const [
          'Aire Acondicionado',
          'Bluetooth',
          'Cámara Trasera',
          'Control Crucero',
          'Sensores de Estacionamiento',
        ],
        safetyFeatures: const ['Airbags', 'ABS', 'Control de Estabilidad'],
        location: 'Santo Domingo, República Dominicana',
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        latitude: 18.4861,
        longitude: -69.9312,
        status: ListingStatus.active,
        views: 324,
        leads: 12,
        favorites: 28,
        rejectionReason: null,
        createdAt: now.subtract(const Duration(days: 15)),
        updatedAt: now.subtract(const Duration(days: 2)),
        publishedAt: now.subtract(const Duration(days: 14)),
        soldAt: null,
        expiresAt: now.add(const Duration(days: 45)),
      ),
      DealerListingModel(
        id: 'listing-${_listingIdCounter++}',
        dealerId: 'dealer-1',
        vehicleId: 'vehicle-2',
        title: 'Honda CR-V 2023 - SUV Premium',
        description:
            'Honda CR-V 2023 modelo EX-L con todas las comodidades. Cuero, techo panorámico, sistema de sonido premium. Perfecto estado.',
        images: const [
          'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6',
          'https://images.unsplash.com/photo-1606664515666-7a86dc27ab93',
        ],
        videoUrl: 'https://www.youtube.com/watch?v=example',
        brand: 'Honda',
        model: 'CR-V',
        year: 2023,
        color: 'Negro',
        mileage: 8500,
        condition: VehicleCondition.certificado,
        vin: 'JHLRD78962C000002',
        price: 32500,
        discountPrice: null,
        currency: 'USD',
        negotiable: false,
        transmission: 'Automática',
        fuelType: 'Híbrido',
        engineSize: '2.0L Turbo',
        doors: 5,
        seats: 5,
        features: const [
          'Aire Acondicionado Dual',
          'Navegación GPS',
          'Techo Panorámico',
          'Asientos de Cuero',
          'Sistema de Sonido Premium',
          'Apple CarPlay',
          'Android Auto',
        ],
        safetyFeatures: const [
          'Honda Sensing',
          '8 Airbags',
          'Asistente de Mantenimiento de Carril',
          'Frenado Automático',
        ],
        location: 'Santiago, República Dominicana',
        city: 'Santiago',
        state: 'Santiago',
        latitude: 19.4517,
        longitude: -70.6971,
        status: ListingStatus.active,
        views: 567,
        leads: 23,
        favorites: 45,
        rejectionReason: null,
        createdAt: now.subtract(const Duration(days: 8)),
        updatedAt: now.subtract(const Duration(hours: 12)),
        publishedAt: now.subtract(const Duration(days: 7)),
        soldAt: null,
        expiresAt: now.add(const Duration(days: 52)),
      ),
      DealerListingModel(
        id: 'listing-${_listingIdCounter++}',
        dealerId: 'dealer-1',
        vehicleId: 'vehicle-3',
        title: 'Nissan Sentra 2021 - Económico',
        description:
            'Nissan Sentra 2021 en excelentes condiciones. Bajo consumo de combustible, ideal para ciudad. Mantenimiento completo.',
        images: const [
          'https://images.unsplash.com/photo-1609521263047-f8f205293f24',
        ],
        videoUrl: null,
        brand: 'Nissan',
        model: 'Sentra',
        year: 2021,
        color: 'Gris',
        mileage: 42000,
        condition: VehicleCondition.usado,
        vin: 'KNMAT2MV5MP000003',
        price: 14200,
        discountPrice: 13500,
        currency: 'USD',
        negotiable: true,
        transmission: 'Manual',
        fuelType: 'Gasolina',
        engineSize: '1.6L',
        doors: 4,
        seats: 5,
        features: const ['Aire Acondicionado', 'Radio AM/FM', 'Bluetooth'],
        safetyFeatures: const ['Airbags Frontales', 'ABS'],
        location: 'La Romana, República Dominicana',
        city: 'La Romana',
        state: 'La Romana',
        latitude: 18.4273,
        longitude: -68.9720,
        status: ListingStatus.pending,
        views: 89,
        leads: 3,
        favorites: 7,
        rejectionReason: null,
        createdAt: now.subtract(const Duration(days: 3)),
        updatedAt: now.subtract(const Duration(days: 1)),
        publishedAt: null,
        soldAt: null,
        expiresAt: null,
      ),
      DealerListingModel(
        id: 'listing-${_listingIdCounter++}',
        dealerId: 'dealer-1',
        vehicleId: 'vehicle-4',
        title: 'Mazda CX-5 2023 - VENDIDO',
        description:
            'Mazda CX-5 2023 Signature - VENDIDO. SUV premium con todas las opciones.',
        images: const [
          'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6',
        ],
        videoUrl: null,
        brand: 'Mazda',
        model: 'CX-5',
        year: 2023,
        color: 'Rojo',
        mileage: 5200,
        condition: VehicleCondition.certificado,
        vin: 'JM3KFACM7M0000004',
        price: 35000,
        discountPrice: null,
        currency: 'USD',
        negotiable: false,
        transmission: 'Automática',
        fuelType: 'Gasolina',
        engineSize: '2.5L Turbo',
        doors: 5,
        seats: 5,
        features: const [
          'Cuero Nappa',
          'Head-Up Display',
          'Sonido Bose',
          'Navegación',
        ],
        safetyFeatures: const ['i-Activsense', '6 Airbags'],
        location: 'Santo Domingo, República Dominicana',
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        latitude: 18.4861,
        longitude: -69.9312,
        status: ListingStatus.sold,
        views: 892,
        leads: 34,
        favorites: 67,
        rejectionReason: null,
        createdAt: now.subtract(const Duration(days: 45)),
        updatedAt: now.subtract(const Duration(days: 5)),
        publishedAt: now.subtract(const Duration(days: 44)),
        soldAt: now.subtract(const Duration(days: 5)),
        expiresAt: null,
      ),
      DealerListingModel(
        id: 'listing-${_listingIdCounter++}',
        dealerId: 'dealer-1',
        vehicleId: 'vehicle-5',
        title: 'Ford Explorer 2024 - Borrador',
        description: 'Nueva Ford Explorer 2024. Descripción pendiente...',
        images: const [],
        videoUrl: null,
        brand: 'Ford',
        model: 'Explorer',
        year: 2024,
        color: 'Azul',
        mileage: 0,
        condition: VehicleCondition.nuevo,
        vin: '1FM5K8HT1RGA00005',
        price: 45000,
        discountPrice: null,
        currency: 'USD',
        negotiable: true,
        transmission: 'Automática',
        fuelType: 'Gasolina',
        engineSize: '3.0L V6',
        doors: 5,
        seats: 7,
        features: const [],
        safetyFeatures: const [],
        location: 'Santo Domingo',
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        latitude: null,
        longitude: null,
        status: ListingStatus.draft,
        views: 0,
        leads: 0,
        favorites: 0,
        rejectionReason: null,
        createdAt: now.subtract(const Duration(hours: 2)),
        updatedAt: now.subtract(const Duration(hours: 1)),
        publishedAt: null,
        soldAt: null,
        expiresAt: null,
      ),
    ]);

    // Create mock leads
    _leads.addAll([
      LeadModel(
        id: 'lead-${_leadIdCounter++}',
        dealerId: 'dealer-1',
        listingId: 'listing-1',
        userId: 'user-1',
        userName: 'Carlos Rodríguez',
        userEmail: 'carlos.rodriguez@email.com',
        userPhone: '+1-809-555-0101',
        userAvatar: 'https://i.pravatar.cc/150?img=12',
        vehicleTitle: 'Toyota Corolla 2022',
        vehicleImage:
            'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb',
        vehiclePrice: 18500,
        status: LeadStatus.negociacion,
        priority: LeadPriority.alta,
        source: LeadSource.mobile,
        message: 'Estoy interesado en el Corolla. ¿Puedo verlo mañana?',
        notes: 'Cliente serio, tiene presupuesto aprobado',
        budget: 19000,
        needsFinancing: false,
        hasTradeIn: true,
        tradeInDetails: 'Hyundai Accent 2018',
        messageCount: 8,
        callCount: 2,
        lastContactedAt: now.subtract(const Duration(hours: 3)),
        nextFollowUpAt: now.add(const Duration(hours: 24)),
        assignedTo: null,
        offeredPrice: 17500,
        lostReason: null,
        convertedAt: null,
        soldPrice: null,
        createdAt: now.subtract(const Duration(days: 4)),
        updatedAt: now.subtract(const Duration(hours: 3)),
      ),
      LeadModel(
        id: 'lead-${_leadIdCounter++}',
        dealerId: 'dealer-1',
        listingId: 'listing-2',
        userId: 'user-2',
        userName: 'María García',
        userEmail: 'maria.garcia@email.com',
        userPhone: '+1-809-555-0102',
        userAvatar: 'https://i.pravatar.cc/150?img=45',
        vehicleTitle: 'Honda CR-V 2023',
        vehicleImage:
            'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6',
        vehiclePrice: 32500,
        status: LeadStatus.nuevo,
        priority: LeadPriority.media,
        source: LeadSource.web,
        message: 'Me gustaría más información sobre el CR-V',
        notes: null,
        budget: null,
        needsFinancing: true,
        hasTradeIn: false,
        tradeInDetails: null,
        messageCount: 1,
        callCount: 0,
        lastContactedAt: null,
        nextFollowUpAt: null,
        assignedTo: null,
        offeredPrice: null,
        lostReason: null,
        convertedAt: null,
        soldPrice: null,
        createdAt: now.subtract(const Duration(hours: 6)),
        updatedAt: now.subtract(const Duration(hours: 6)),
      ),
      LeadModel(
        id: 'lead-${_leadIdCounter++}',
        dealerId: 'dealer-1',
        listingId: 'listing-1',
        userId: 'user-3',
        userName: 'Pedro Martínez',
        userEmail: 'pedro.martinez@email.com',
        userPhone: '+1-809-555-0103',
        userAvatar: null,
        vehicleTitle: 'Toyota Corolla 2022',
        vehicleImage:
            'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb',
        vehiclePrice: 18500,
        status: LeadStatus.contactado,
        priority: LeadPriority.urgente,
        source: LeadSource.llamada,
        message: null,
        notes: 'Llamó directamente, muy interesado',
        budget: 18000,
        needsFinancing: false,
        hasTradeIn: false,
        tradeInDetails: null,
        messageCount: 0,
        callCount: 1,
        lastContactedAt: now.subtract(const Duration(days: 1)),
        nextFollowUpAt: now.add(const Duration(hours: 2)),
        assignedTo: null,
        offeredPrice: null,
        lostReason: null,
        convertedAt: null,
        soldPrice: null,
        createdAt: now.subtract(const Duration(days: 2)),
        updatedAt: now.subtract(const Duration(days: 1)),
      ),
    ]);
  }

  @override
  Future<Either<Failure, DealerStats>> getDealerStats(String dealerId) async {
    await Future.delayed(const Duration(milliseconds: 500));

    try {
      final activeListings = _listings.where((l) => l.isActive).length;
      final totalViews = _listings.fold<int>(0, (sum, l) => sum + l.views);
      final totalLeads = _listings.fold<int>(0, (sum, l) => sum + l.leads);
      final totalConversions = _listings.where((l) => l.isSold).length;

      final stats = DealerStatsModel(
        totalListings: _listings.length,
        activeListings: activeListings,
        pendingListings: _listings.where((l) => l.isPending).length,
        soldListings: _listings.where((l) => l.isSold).length,
        totalViews: totalViews,
        totalLeads: totalLeads,
        totalConversions: totalConversions,
        conversionRate: totalViews > 0 ? (totalLeads / totalViews) * 100 : 0.0,
        averageResponseTime: 2.5,
        revenue: 35000,
        monthlyRevenue: 12500,
        viewsByMonth: const {
          'Ene': 145,
          'Feb': 189,
          'Mar': 234,
          'Abr': 298,
          'May': 356,
          'Jun': 412,
        },
        leadsByMonth: const {
          'Ene': 12,
          'Feb': 18,
          'Mar': 24,
          'Abr': 31,
          'May': 42,
          'Jun': 56,
        },
        topPerformingVehicles: const [
          TopPerformingVehicleModel(
            vehicleId: 'vehicle-2',
            title: 'Honda CR-V 2023',
            imageUrl:
                'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6',
            views: 567,
            leads: 23,
            conversionRate: 4.06,
          ),
          TopPerformingVehicleModel(
            vehicleId: 'vehicle-1',
            title: 'Toyota Corolla 2022',
            imageUrl:
                'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb',
            views: 324,
            leads: 12,
            conversionRate: 3.70,
          ),
        ],
      );

      return Right(stats);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<DealerListing>>> getListings({
    required String dealerId,
    ListingStatus? status,
    String? searchQuery,
    int? limit,
    int? offset,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      var filtered = _listings.where((l) => l.dealerId == dealerId);

      if (status != null) {
        filtered = filtered.where((l) => l.status == status);
      }

      if (searchQuery != null && searchQuery.isNotEmpty) {
        final query = searchQuery.toLowerCase();
        filtered = filtered.where((l) =>
            l.title.toLowerCase().contains(query) ||
            l.brand.toLowerCase().contains(query) ||
            l.model.toLowerCase().contains(query));
      }

      var result = filtered.toList()
        ..sort((a, b) => b.updatedAt.compareTo(a.updatedAt));

      if (offset != null) {
        result = result.skip(offset).toList();
      }

      if (limit != null) {
        result = result.take(limit).toList();
      }

      return Right(result);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, DealerListing>> getListingById(
    String listingId,
  ) async {
    await Future.delayed(const Duration(milliseconds: 300));

    try {
      final listing = _listings.firstWhere((l) => l.id == listingId);
      return Right(listing);
    } catch (e) {
      return const Left(NotFoundFailure(message: 'Listing not found'));
    }
  }

  @override
  Future<Either<Failure, DealerListing>> createListing(
    DealerListing listing,
  ) async {
    await Future.delayed(const Duration(milliseconds: 600));

    try {
      final newListing = DealerListingModel.fromEntity(listing).copyWith(
        id: 'listing-${_listingIdCounter++}',
        createdAt: DateTime.now(),
        updatedAt: DateTime.now(),
      );

      _listings.add(newListing as DealerListingModel);
      return Right(newListing);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, DealerListing>> updateListing(
    DealerListing listing,
  ) async {
    await Future.delayed(const Duration(milliseconds: 500));

    try {
      final index = _listings.indexWhere((l) => l.id == listing.id);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Listing not found'));
      }

      final updated = DealerListingModel.fromEntity(listing).copyWith(
        updatedAt: DateTime.now(),
      );

      _listings[index] = updated as DealerListingModel;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> deleteListing(String listingId) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      _listings.removeWhere((l) => l.id == listingId);
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> bulkUpdateListingStatus({
    required List<String> listingIds,
    required ListingStatus status,
  }) async {
    await Future.delayed(const Duration(milliseconds: 700));

    try {
      for (final id in listingIds) {
        final index = _listings.indexWhere((l) => l.id == id);
        if (index != -1) {
          _listings[index] = _listings[index].copyWith(
            status: status,
            updatedAt: DateTime.now(),
          ) as DealerListingModel;
        }
      }
      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, DealerListing>> publishListing(
    String listingId,
  ) async {
    await Future.delayed(const Duration(milliseconds: 500));

    try {
      final index = _listings.indexWhere((l) => l.id == listingId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Listing not found'));
      }

      final updated = _listings[index].copyWith(
        status: ListingStatus.active,
        publishedAt: DateTime.now(),
        updatedAt: DateTime.now(),
        expiresAt: DateTime.now().add(const Duration(days: 60)),
      ) as DealerListingModel;

      _listings[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, DealerListing>> markListingAsSold({
    required String listingId,
    required double soldPrice,
  }) async {
    await Future.delayed(const Duration(milliseconds: 500));

    try {
      final index = _listings.indexWhere((l) => l.id == listingId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Listing not found'));
      }

      final updated = _listings[index].copyWith(
        status: ListingStatus.sold,
        soldAt: DateTime.now(),
        updatedAt: DateTime.now(),
      ) as DealerListingModel;

      _listings[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Lead>>> getLeads({
    required String dealerId,
    LeadStatus? status,
    LeadPriority? priority,
    String? searchQuery,
    int? limit,
    int? offset,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      var filtered = _leads.where((l) => l.dealerId == dealerId);

      if (status != null) {
        filtered = filtered.where((l) => l.status == status);
      }

      if (priority != null) {
        filtered = filtered.where((l) => l.priority == priority);
      }

      if (searchQuery != null && searchQuery.isNotEmpty) {
        final query = searchQuery.toLowerCase();
        filtered = filtered.where((l) =>
            l.userName.toLowerCase().contains(query) ||
            l.vehicleTitle.toLowerCase().contains(query));
      }

      var result = filtered.toList()
        ..sort((a, b) => b.updatedAt.compareTo(a.updatedAt));

      if (offset != null) {
        result = result.skip(offset).toList();
      }

      if (limit != null) {
        result = result.take(limit).toList();
      }

      return Right(result);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> getLeadById(String leadId) async {
    await Future.delayed(const Duration(milliseconds: 300));

    try {
      final lead = _leads.firstWhere((l) => l.id == leadId);
      return Right(lead);
    } catch (e) {
      return const Left(NotFoundFailure(message: 'Lead not found'));
    }
  }

  @override
  Future<Either<Failure, Lead>> updateLeadStatus({
    required String leadId,
    required LeadStatus status,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        status: status,
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> updateLeadPriority({
    required String leadId,
    required LeadPriority priority,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        priority: priority,
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> updateLeadNotes({
    required String leadId,
    required String notes,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        notes: notes,
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> scheduleFollowUp({
    required String leadId,
    required DateTime followUpDate,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        nextFollowUpAt: followUpDate,
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> convertLead({
    required String leadId,
    required double soldPrice,
  }) async {
    await Future.delayed(const Duration(milliseconds: 500));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        status: LeadStatus.ganado,
        soldPrice: soldPrice,
        convertedAt: DateTime.now(),
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Lead>> markLeadAsLost({
    required String leadId,
    required String reason,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final index = _leads.indexWhere((l) => l.id == leadId);
      if (index == -1) {
        return const Left(NotFoundFailure(message: 'Lead not found'));
      }

      final updated = _leads[index].copyWith(
        status: LeadStatus.perdido,
        lostReason: reason,
        updatedAt: DateTime.now(),
      ) as LeadModel;

      _leads[index] = updated;
      return Right(updated);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Map<String, int>>> getViewsByMonth({
    required String dealerId,
    required int year,
  }) async {
    await Future.delayed(const Duration(milliseconds: 300));

    return const Right({
      'Ene': 145,
      'Feb': 189,
      'Mar': 234,
      'Abr': 298,
      'May': 356,
      'Jun': 412,
    });
  }

  @override
  Future<Either<Failure, Map<String, int>>> getLeadsByMonth({
    required String dealerId,
    required int year,
  }) async {
    await Future.delayed(const Duration(milliseconds: 300));

    return const Right({
      'Ene': 12,
      'Feb': 18,
      'Mar': 24,
      'Abr': 31,
      'May': 42,
      'Jun': 56,
    });
  }

  @override
  Future<Either<Failure, List<DealerListing>>> getTopPerformingListings({
    required String dealerId,
    int limit = 10,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    try {
      final listings = _listings
          .where((l) => l.dealerId == dealerId && l.isActive)
          .toList()
        ..sort((a, b) => b.conversionRate.compareTo(a.conversionRate));

      return Right(listings.take(limit).toList());
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }
}
