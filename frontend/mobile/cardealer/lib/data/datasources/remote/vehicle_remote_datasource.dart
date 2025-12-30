import '../../../core/config/api_config.dart';
import '../../../core/network/api_client.dart';
import '../../models/vehicle_model.dart';

/// Remote datasource for vehicles
/// Communicates with the real API backend
class VehicleRemoteDataSource {
  final ApiClient _apiClient;
  final String _baseUrl;

  VehicleRemoteDataSource({
    required ApiClient apiClient,
    String? baseUrl,
  })  : _apiClient = apiClient,
        _baseUrl = baseUrl ?? ApiConfig.vehicleServiceUrl;

  /// Get vehicles for hero carousel (featured/promoted)
  Future<List<VehicleModel>> getHeroCarouselVehicles() async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/featured/hero',
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load hero vehicles');
  }

  /// Get vehicles for featured grid
  Future<List<VehicleModel>> getFeaturedGridVehicles({int limit = 6}) async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/featured/grid',
      queryParameters: {'limit': limit},
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load featured vehicles');
  }

  /// Get this week's featured vehicles
  Future<List<VehicleModel>> getWeekFeaturedVehicles() async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/featured/weekly',
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load weekly featured');
  }

  /// Get daily deals
  Future<List<VehicleModel>> getDailyDeals() async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/deals/daily',
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load daily deals');
  }

  /// Get SUVs and Trucks
  Future<List<VehicleModel>> getSUVsAndTrucks({int limit = 10}) async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/category/suv-truck',
      queryParameters: {'limit': limit},
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load SUVs and trucks');
  }

  /// Get premium/luxury vehicles
  Future<List<VehicleModel>> getPremiumVehicles({int limit = 10}) async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/category/premium',
      queryParameters: {'limit': limit},
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load premium vehicles');
  }

  /// Get electric and hybrid vehicles
  Future<List<VehicleModel>> getElectricAndHybrid({int limit = 10}) async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/category/electric-hybrid',
      queryParameters: {'limit': limit},
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load electric vehicles');
  }

  /// Get vehicle by ID
  Future<VehicleModel> getVehicleById(String id) async {
    final response = await _apiClient.get<VehicleModel>(
      '$_baseUrl/$id',
      fromJson: (json) => VehicleModel.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Vehicle not found');
  }

  /// Search vehicles with filters
  Future<PaginatedVehicleResponse> searchVehicles({
    String? query,
    String? make,
    String? model,
    int? yearFrom,
    int? yearTo,
    double? priceFrom,
    double? priceTo,
    String? condition,
    String? transmission,
    String? fuelType,
    String? bodyType,
    String? location,
    double? radius,
    String? sortBy,
    bool? sortDescending,
    int page = 1,
    int pageSize = 20,
  }) async {
    final queryParams = <String, dynamic>{
      if (query != null && query.isNotEmpty) 'query': query,
      if (make != null) 'make': make,
      if (model != null) 'model': model,
      if (yearFrom != null) 'yearFrom': yearFrom,
      if (yearTo != null) 'yearTo': yearTo,
      if (priceFrom != null) 'priceFrom': priceFrom,
      if (priceTo != null) 'priceTo': priceTo,
      if (condition != null) 'condition': condition,
      if (transmission != null) 'transmission': transmission,
      if (fuelType != null) 'fuelType': fuelType,
      if (bodyType != null) 'bodyType': bodyType,
      if (location != null) 'location': location,
      if (radius != null) 'radius': radius,
      if (sortBy != null) 'sortBy': sortBy,
      if (sortDescending != null) 'sortDescending': sortDescending,
      'page': page,
      'pageSize': pageSize,
    };

    final response = await _apiClient.get<PaginatedVehicleResponse>(
      '$_baseUrl/search',
      queryParameters: queryParams,
      fromJson: (json) => PaginatedVehicleResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Search failed');
  }

  /// Get all vehicles with pagination
  Future<PaginatedVehicleResponse> getVehicles({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    bool sortDescending = true,
  }) async {
    final response = await _apiClient.get<PaginatedVehicleResponse>(
      _baseUrl,
      queryParameters: {
        'page': page,
        'pageSize': pageSize,
        if (sortBy != null) 'sortBy': sortBy,
        'sortDescending': sortDescending,
      },
      fromJson: (json) => PaginatedVehicleResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load vehicles');
  }

  /// Get similar vehicles to a given vehicle
  Future<List<VehicleModel>> getSimilarVehicles(String vehicleId, {int limit = 6}) async {
    final response = await _apiClient.get<List<VehicleModel>>(
      '$_baseUrl/$vehicleId/similar',
      queryParameters: {'limit': limit},
      fromJson: (json) => (json as List)
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load similar vehicles');
  }

  /// Get vehicle makes (for filter options)
  Future<List<String>> getMakes() async {
    final response = await _apiClient.get<List<String>>(
      '$_baseUrl/filters/makes',
      fromJson: (json) => (json as List).map((e) => e.toString()).toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load makes');
  }

  /// Get vehicle models for a make
  Future<List<String>> getModels(String make) async {
    final response = await _apiClient.get<List<String>>(
      '$_baseUrl/filters/models',
      queryParameters: {'make': make},
      fromJson: (json) => (json as List).map((e) => e.toString()).toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load models');
  }

  /// Get body types (for filter options)
  Future<List<String>> getBodyTypes() async {
    final response = await _apiClient.get<List<String>>(
      '$_baseUrl/filters/body-types',
      fromJson: (json) => (json as List).map((e) => e.toString()).toList(),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to load body types');
  }

  /// Contact seller for a vehicle
  Future<void> contactSeller({
    required String vehicleId,
    required String name,
    required String email,
    required String phone,
    required String message,
  }) async {
    final response = await _apiClient.post(
      '$_baseUrl/$vehicleId/contact',
      data: {
        'name': name,
        'email': email,
        'phone': phone,
        'message': message,
      },
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Failed to send message');
    }
  }

  /// Report a vehicle listing
  Future<void> reportVehicle({
    required String vehicleId,
    required String reason,
    String? details,
  }) async {
    final response = await _apiClient.post(
      '$_baseUrl/$vehicleId/report',
      data: {
        'reason': reason,
        if (details != null) 'details': details,
      },
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Failed to report vehicle');
    }
  }
}

/// Paginated response for vehicle listings
class PaginatedVehicleResponse {
  final List<VehicleModel> items;
  final int totalCount;
  final int page;
  final int pageSize;
  final int totalPages;
  final bool hasNextPage;
  final bool hasPreviousPage;

  PaginatedVehicleResponse({
    required this.items,
    required this.totalCount,
    required this.page,
    required this.pageSize,
    required this.totalPages,
    required this.hasNextPage,
    required this.hasPreviousPage,
  });

  factory PaginatedVehicleResponse.fromJson(Map<String, dynamic> json) {
    return PaginatedVehicleResponse(
      items: (json['items'] as List? ?? json['data'] as List? ?? [])
          .map((item) => VehicleModel.fromJson(item))
          .toList(),
      totalCount: json['totalCount'] ?? json['total'] ?? 0,
      page: json['page'] ?? json['currentPage'] ?? 1,
      pageSize: json['pageSize'] ?? json['limit'] ?? 20,
      totalPages: json['totalPages'] ?? 1,
      hasNextPage: json['hasNextPage'] ?? false,
      hasPreviousPage: json['hasPreviousPage'] ?? false,
    );
  }
}

/// API Exception for error handling
class ApiException implements Exception {
  final String message;
  final int? statusCode;
  final Map<String, dynamic>? errors;

  ApiException(this.message, {this.statusCode, this.errors});

  @override
  String toString() => message;
}
