import '../../domain/entities/vehicle.dart';

/// Vehicle model for JSON serialization
class VehicleModel extends Vehicle {
  const VehicleModel({
    required super.id,
    required super.make,
    required super.model,
    required super.year,
    required super.price,
    required super.mileage,
    required super.condition,
    required super.transmission,
    required super.fuelType,
    required super.bodyType,
    super.color,
    super.vin,
    required super.images,
    required super.description,
    required super.location,
    super.dealerId,
    super.dealerName,
    required super.isFeatured,
    required super.isVerified,
    required super.createdAt,
    super.updatedAt,
    super.doors,
    super.seats,
    super.engineSize,
    super.horsepower,
    super.drivetrain,
    super.features,
  });

  /// Create from JSON
  factory VehicleModel.fromJson(Map<String, dynamic> json) {
    return VehicleModel(
      id: json['id'] as String,
      make: json['make'] as String,
      model: json['model'] as String,
      year: json['year'] as int,
      price: (json['price'] as num).toDouble(),
      mileage: json['mileage'] as int,
      condition: json['condition'] as String,
      transmission: json['transmission'] as String,
      fuelType: json['fuelType'] as String,
      bodyType: json['bodyType'] as String,
      color: json['color'] as String?,
      vin: json['vin'] as String?,
      images: (json['images'] as List<dynamic>).map((e) => e as String).toList(),
      description: json['description'] as String,
      location: json['location'] as String,
      dealerId: json['dealerId'] as String?,
      dealerName: json['dealerName'] as String?,
      isFeatured: json['isFeatured'] as bool,
      isVerified: json['isVerified'] as bool,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null
          ? DateTime.parse(json['updatedAt'] as String)
          : null,
      doors: json['doors'] as int?,
      seats: json['seats'] as int?,
      engineSize: json['engineSize'] as String?,
      horsepower: json['horsepower'] as int?,
      drivetrain: json['drivetrain'] as String?,
      features: json['features'] != null
          ? (json['features'] as List<dynamic>).map((e) => e as String).toList()
          : [],
    );
  }

  /// Convert to JSON
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'make': make,
      'model': model,
      'year': year,
      'price': price,
      'mileage': mileage,
      'condition': condition,
      'transmission': transmission,
      'fuelType': fuelType,
      'bodyType': bodyType,
      'color': color,
      'vin': vin,
      'images': images,
      'description': description,
      'location': location,
      'dealerId': dealerId,
      'dealerName': dealerName,
      'isFeatured': isFeatured,
      'isVerified': isVerified,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
      'doors': doors,
      'seats': seats,
      'engineSize': engineSize,
      'horsepower': horsepower,
      'drivetrain': drivetrain,
      'features': features,
    };
  }

  /// Convert domain entity to model
  factory VehicleModel.fromEntity(Vehicle vehicle) {
    return VehicleModel(
      id: vehicle.id,
      make: vehicle.make,
      model: vehicle.model,
      year: vehicle.year,
      price: vehicle.price,
      mileage: vehicle.mileage,
      condition: vehicle.condition,
      transmission: vehicle.transmission,
      fuelType: vehicle.fuelType,
      bodyType: vehicle.bodyType,
      color: vehicle.color,
      vin: vehicle.vin,
      images: vehicle.images,
      description: vehicle.description,
      location: vehicle.location,
      dealerId: vehicle.dealerId,
      dealerName: vehicle.dealerName,
      isFeatured: vehicle.isFeatured,
      isVerified: vehicle.isVerified,
      createdAt: vehicle.createdAt,
      updatedAt: vehicle.updatedAt,
      doors: vehicle.doors,
      seats: vehicle.seats,
      engineSize: vehicle.engineSize,
      horsepower: vehicle.horsepower,
      drivetrain: vehicle.drivetrain,
      features: vehicle.features,
    );
  }
}
