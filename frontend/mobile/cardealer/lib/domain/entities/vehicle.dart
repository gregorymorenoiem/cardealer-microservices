import 'package:equatable/equatable.dart';

/// Vehicle entity representing a car listing
class Vehicle extends Equatable {
  final String id;
  final String make;
  final String model;
  final int year;
  final double price;
  final int mileage;
  final String condition; // 'new', 'used', 'certified'
  final String transmission; // 'automatic', 'manual'
  final String fuelType; // 'gasoline', 'diesel', 'electric', 'hybrid'
  final String bodyType; // 'sedan', 'suv', 'truck', 'coupe', etc.
  final String? color;
  final String? vin;
  final List<String> images;
  final String description;
  final String location;
  final String? dealerId;
  final String? dealerName;
  final bool isFeatured;
  final bool isVerified;
  final DateTime createdAt;
  final DateTime? updatedAt;
  
  // Additional specs
  final int? doors;
  final int? seats;
  final String? engineSize;
  final int? horsepower;
  final String? drivetrain; // 'fwd', 'rwd', 'awd', '4wd'
  
  // Features
  final List<String> features;
  
  const Vehicle({
    required this.id,
    required this.make,
    required this.model,
    required this.year,
    required this.price,
    required this.mileage,
    required this.condition,
    required this.transmission,
    required this.fuelType,
    required this.bodyType,
    this.color,
    this.vin,
    required this.images,
    required this.description,
    required this.location,
    this.dealerId,
    this.dealerName,
    required this.isFeatured,
    required this.isVerified,
    required this.createdAt,
    this.updatedAt,
    this.doors,
    this.seats,
    this.engineSize,
    this.horsepower,
    this.drivetrain,
    this.features = const [],
  });

  /// Get vehicle title (make + model + year)
  String get title => '$year $make $model';

  /// Get formatted price
  String get formattedPrice => '\$${price.toStringAsFixed(0)}';

  /// Get formatted mileage
  String get formattedMileage => '${mileage.toString().replaceAllMapped(
        RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
        (Match m) => '${m[1]},',
      )} km';

  /// Check if vehicle is new
  bool get isNew => condition.toLowerCase() == 'new';

  /// Check if vehicle is certified
  bool get isCertified => condition.toLowerCase() == 'certified';

  /// Get main image URL
  String get mainImage => images.isNotEmpty ? images.first : '';

  /// Copy with method for immutability
  Vehicle copyWith({
    String? id,
    String? make,
    String? model,
    int? year,
    double? price,
    int? mileage,
    String? condition,
    String? transmission,
    String? fuelType,
    String? bodyType,
    String? color,
    String? vin,
    List<String>? images,
    String? description,
    String? location,
    String? dealerId,
    String? dealerName,
    bool? isFeatured,
    bool? isVerified,
    DateTime? createdAt,
    DateTime? updatedAt,
    int? doors,
    int? seats,
    String? engineSize,
    int? horsepower,
    String? drivetrain,
    List<String>? features,
  }) {
    return Vehicle(
      id: id ?? this.id,
      make: make ?? this.make,
      model: model ?? this.model,
      year: year ?? this.year,
      price: price ?? this.price,
      mileage: mileage ?? this.mileage,
      condition: condition ?? this.condition,
      transmission: transmission ?? this.transmission,
      fuelType: fuelType ?? this.fuelType,
      bodyType: bodyType ?? this.bodyType,
      color: color ?? this.color,
      vin: vin ?? this.vin,
      images: images ?? this.images,
      description: description ?? this.description,
      location: location ?? this.location,
      dealerId: dealerId ?? this.dealerId,
      dealerName: dealerName ?? this.dealerName,
      isFeatured: isFeatured ?? this.isFeatured,
      isVerified: isVerified ?? this.isVerified,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      doors: doors ?? this.doors,
      seats: seats ?? this.seats,
      engineSize: engineSize ?? this.engineSize,
      horsepower: horsepower ?? this.horsepower,
      drivetrain: drivetrain ?? this.drivetrain,
      features: features ?? this.features,
    );
  }

  @override
  List<Object?> get props => [
        id,
        make,
        model,
        year,
        price,
        mileage,
        condition,
        transmission,
        fuelType,
        bodyType,
        color,
        vin,
        images,
        description,
        location,
        dealerId,
        dealerName,
        isFeatured,
        isVerified,
        createdAt,
        updatedAt,
        doors,
        seats,
        engineSize,
        horsepower,
        drivetrain,
        features,
      ];
}

/// Vehicle condition types
enum VehicleCondition {
  newCar,
  used,
  certified,
}

/// Extension for VehicleCondition
extension VehicleConditionExtension on VehicleCondition {
  String get value {
    switch (this) {
      case VehicleCondition.newCar:
        return 'new';
      case VehicleCondition.used:
        return 'used';
      case VehicleCondition.certified:
        return 'certified';
    }
  }

  String get displayName {
    switch (this) {
      case VehicleCondition.newCar:
        return 'New';
      case VehicleCondition.used:
        return 'Used';
      case VehicleCondition.certified:
        return 'Certified Pre-Owned';
    }
  }
}
