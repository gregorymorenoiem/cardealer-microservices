/// Remote datasource for vehicles
/// This will communicate with the real API
/// Currently inactive - using mock data instead
class VehicleRemoteDataSource {
  // TODO: Implement when API is ready

  Future<List<dynamic>> getHeroCarouselVehicles() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getFeaturedGridVehicles() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getWeekFeaturedVehicles() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getDailyDeals() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getSUVsAndTrucks() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getPremiumVehicles() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> getElectricAndHybrid() async {
    throw UnimplementedError('API not ready yet');
  }

  Future<dynamic> getVehicleById(String id) async {
    throw UnimplementedError('API not ready yet');
  }

  Future<List<dynamic>> searchVehicles({
    String? query,
    String? make,
    String? model,
    int? yearFrom,
    int? yearTo,
    double? priceFrom,
    double? priceTo,
  }) async {
    throw UnimplementedError('API not ready yet');
  }
}
