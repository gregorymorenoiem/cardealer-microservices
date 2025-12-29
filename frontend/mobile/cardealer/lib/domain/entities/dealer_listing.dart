import 'package:equatable/equatable.dart';

/// Status of a dealer listing
enum ListingStatus {
  draft,
  active,
  pending,
  sold,
  expired,
  rejected,
}

/// Condition of the vehicle
enum VehicleCondition {
  nuevo,
  usado,
  certificado,
}

/// Represents a vehicle listing created by a dealer
class DealerListing extends Equatable {
  final String id;
  final String dealerId;
  final String vehicleId;

  // Basic info
  final String title;
  final String description;
  final List<String> images;
  final String? videoUrl;

  // Vehicle details
  final String brand;
  final String model;
  final int year;
  final String color;
  final int mileage;
  final VehicleCondition condition;
  final String? vin;

  // Pricing
  final double price;
  final double? discountPrice;
  final String currency;
  final bool negotiable;

  // Technical specs
  final String transmission;
  final String fuelType;
  final String? engineSize;
  final int? doors;
  final int? seats;

  // Features
  final List<String> features;
  final List<String> safetyFeatures;

  // Location
  final String location;
  final String? city;
  final String? state;
  final double? latitude;
  final double? longitude;

  // Status and metrics
  final ListingStatus status;
  final int views;
  final int leads;
  final int favorites;
  final String? rejectionReason;

  // Timestamps
  final DateTime createdAt;
  final DateTime updatedAt;
  final DateTime? publishedAt;
  final DateTime? soldAt;
  final DateTime? expiresAt;

  const DealerListing({
    required this.id,
    required this.dealerId,
    required this.vehicleId,
    required this.title,
    required this.description,
    required this.images,
    this.videoUrl,
    required this.brand,
    required this.model,
    required this.year,
    required this.color,
    required this.mileage,
    required this.condition,
    this.vin,
    required this.price,
    this.discountPrice,
    required this.currency,
    required this.negotiable,
    required this.transmission,
    required this.fuelType,
    this.engineSize,
    this.doors,
    this.seats,
    required this.features,
    required this.safetyFeatures,
    required this.location,
    this.city,
    this.state,
    this.latitude,
    this.longitude,
    required this.status,
    required this.views,
    required this.leads,
    required this.favorites,
    this.rejectionReason,
    required this.createdAt,
    required this.updatedAt,
    this.publishedAt,
    this.soldAt,
    this.expiresAt,
  });

  @override
  List<Object?> get props => [
        id,
        dealerId,
        vehicleId,
        title,
        description,
        images,
        videoUrl,
        brand,
        model,
        year,
        color,
        mileage,
        condition,
        vin,
        price,
        discountPrice,
        currency,
        negotiable,
        transmission,
        fuelType,
        engineSize,
        doors,
        seats,
        features,
        safetyFeatures,
        location,
        city,
        state,
        latitude,
        longitude,
        status,
        views,
        leads,
        favorites,
        rejectionReason,
        createdAt,
        updatedAt,
        publishedAt,
        soldAt,
        expiresAt,
      ];

  // Helper getters
  bool get isDraft => status == ListingStatus.draft;
  bool get isActive => status == ListingStatus.active;
  bool get isPending => status == ListingStatus.pending;
  bool get isSold => status == ListingStatus.sold;
  bool get isExpired => status == ListingStatus.expired;
  bool get isRejected => status == ListingStatus.rejected;

  bool get hasDiscount => discountPrice != null && discountPrice! < price;

  double get finalPrice => hasDiscount ? discountPrice! : price;

  double get discountPercentage {
    if (!hasDiscount) return 0.0;
    return ((price - discountPrice!) / price) * 100;
  }

  String get priceFormatted => '\$$currency ${finalPrice.toStringAsFixed(0)}';

  String get fullLocation {
    if (city != null && state != null) {
      return '$city, $state';
    }
    return location;
  }

  bool get hasLocation => latitude != null && longitude != null;

  int get daysActive {
    final startDate = publishedAt ?? createdAt;
    return DateTime.now().difference(startDate).inDays;
  }

  bool get isExpiringSoon {
    if (expiresAt == null) return false;
    final daysUntilExpiry = expiresAt!.difference(DateTime.now()).inDays;
    return daysUntilExpiry <= 7 && daysUntilExpiry > 0;
  }

  double get conversionRate {
    if (views == 0) return 0.0;
    return (leads / views) * 100;
  }

  DealerListing copyWith({
    String? id,
    String? dealerId,
    String? vehicleId,
    String? title,
    String? description,
    List<String>? images,
    String? videoUrl,
    String? brand,
    String? model,
    int? year,
    String? color,
    int? mileage,
    VehicleCondition? condition,
    String? vin,
    double? price,
    double? discountPrice,
    String? currency,
    bool? negotiable,
    String? transmission,
    String? fuelType,
    String? engineSize,
    int? doors,
    int? seats,
    List<String>? features,
    List<String>? safetyFeatures,
    String? location,
    String? city,
    String? state,
    double? latitude,
    double? longitude,
    ListingStatus? status,
    int? views,
    int? leads,
    int? favorites,
    String? rejectionReason,
    DateTime? createdAt,
    DateTime? updatedAt,
    DateTime? publishedAt,
    DateTime? soldAt,
    DateTime? expiresAt,
  }) {
    return DealerListing(
      id: id ?? this.id,
      dealerId: dealerId ?? this.dealerId,
      vehicleId: vehicleId ?? this.vehicleId,
      title: title ?? this.title,
      description: description ?? this.description,
      images: images ?? this.images,
      videoUrl: videoUrl ?? this.videoUrl,
      brand: brand ?? this.brand,
      model: model ?? this.model,
      year: year ?? this.year,
      color: color ?? this.color,
      mileage: mileage ?? this.mileage,
      condition: condition ?? this.condition,
      vin: vin ?? this.vin,
      price: price ?? this.price,
      discountPrice: discountPrice ?? this.discountPrice,
      currency: currency ?? this.currency,
      negotiable: negotiable ?? this.negotiable,
      transmission: transmission ?? this.transmission,
      fuelType: fuelType ?? this.fuelType,
      engineSize: engineSize ?? this.engineSize,
      doors: doors ?? this.doors,
      seats: seats ?? this.seats,
      features: features ?? this.features,
      safetyFeatures: safetyFeatures ?? this.safetyFeatures,
      location: location ?? this.location,
      city: city ?? this.city,
      state: state ?? this.state,
      latitude: latitude ?? this.latitude,
      longitude: longitude ?? this.longitude,
      status: status ?? this.status,
      views: views ?? this.views,
      leads: leads ?? this.leads,
      favorites: favorites ?? this.favorites,
      rejectionReason: rejectionReason ?? this.rejectionReason,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      publishedAt: publishedAt ?? this.publishedAt,
      soldAt: soldAt ?? this.soldAt,
      expiresAt: expiresAt ?? this.expiresAt,
    );
  }
}
