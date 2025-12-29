import '../../models/vehicle_model.dart';

/// Mock vehicle datasource with 71+ vehicles for HomePage sections
class MockVehicleDataSource {
  // Simulated API delay
  static const _apiDelay = Duration(milliseconds: 800);

  /// Get hero carousel vehicles (5 featured vehicles)
  Future<List<VehicleModel>> getHeroCarouselVehicles() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles.where((v) => v.isFeatured).take(5).toList();
  }

  /// Get featured grid vehicles (6 vehicles for grid)
  Future<List<VehicleModel>> getFeaturedGridVehicles() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles.where((v) => v.isFeatured).skip(5).take(6).toList();
  }

  /// Get week's featured vehicles (10 vehicles)
  Future<List<VehicleModel>> getWeekFeaturedVehicles() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles.where((v) => v.isFeatured).skip(11).take(10).toList();
  }

  /// Get daily deals (10 vehicles with price < 30000)
  Future<List<VehicleModel>> getDailyDeals() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles.where((v) => v.price < 30000).take(10).toList();
  }

  /// Get SUVs and Trucks (10 vehicles)
  Future<List<VehicleModel>> getSUVsAndTrucks() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles
        .where((v) => v.bodyType == 'suv' || v.bodyType == 'truck')
        .take(10)
        .toList();
  }

  /// Get premium vehicles (10 vehicles with price > 50000)
  Future<List<VehicleModel>> getPremiumVehicles() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles.where((v) => v.price > 50000).take(10).toList();
  }

  /// Get electric and hybrid vehicles (10 vehicles)
  Future<List<VehicleModel>> getElectricAndHybrid() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles
        .where((v) => v.fuelType == 'electric' || v.fuelType == 'hybrid')
        .take(10)
        .toList();
  }

  /// Get all vehicles
  Future<List<VehicleModel>> getAllVehicles() async {
    await Future.delayed(_apiDelay);
    return _mockVehicles;
  }

  /// Mock vehicles data (71+ vehicles)
  static final List<VehicleModel> _mockVehicles = [
    // Hero Carousel + Featured Grid (11 vehicles)
    VehicleModel(
      id: '1',
      make: 'Tesla',
      model: 'Model S',
      year: 2024,
      price: 89990,
      mileage: 150,
      condition: 'new',
      transmission: 'automatic',
      fuelType: 'electric',
      bodyType: 'sedan',
      color: 'Pearl White',
      images: const [
        'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800',
        'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800',
      ],
      description: 'Tesla Model S nuevo con Autopilot e interior premium',
      location: 'Santo Domingo',
      dealerName: 'Tesla RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 1)),
      doors: 4,
      seats: 5,
      horsepower: 670,
      drivetrain: 'awd',
      features: const [
        'Autopilot',
        'Premium Audio',
        'Panoramic Roof',
        'Heated Seats'
      ],
    ),
    VehicleModel(
      id: '2',
      make: 'BMW',
      model: 'X5',
      year: 2023,
      price: 67500,
      mileage: 8500,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'suv',
      color: 'Black Sapphire',
      images: const [
        'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800',
      ],
      description: 'SUV de lujo con paquete M Sport y características premium',
      location: 'Punta Cana',
      dealerName: 'BMW Premium RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 2)),
      doors: 4,
      seats: 7,
      engineSize: '3.0L',
      horsepower: 335,
      drivetrain: 'awd',
      features: const [
        'M Sport Package',
        'Panoramic Sunroof',
        'Navigation',
        'Leather'
      ],
    ),
    VehicleModel(
      id: '3',
      make: 'Mercedes-Benz',
      model: 'E-Class',
      year: 2024,
      price: 62000,
      mileage: 0,
      condition: 'new',
      transmission: 'automatic',
      fuelType: 'hybrid',
      bodyType: 'sedan',
      color: 'Obsidian Black',
      images: const [
        'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800',
      ],
      description: 'Mercedes E-Class nuevo con tecnología híbrida',
      location: 'Santiago',
      dealerName: 'Mercedes RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 3)),
      doors: 4,
      seats: 5,
      engineSize: '2.0L',
      horsepower: 255,
      drivetrain: 'rwd',
      features: const ['Hybrid System', 'MBUX', 'Burmester Audio', 'AMG Line'],
    ),
    VehicleModel(
      id: '4',
      make: 'Porsche',
      model: 'Cayenne',
      year: 2023,
      price: 78900,
      mileage: 12000,
      condition: 'certified',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'suv',
      color: 'Carmine Red',
      images: const [
        'https://images.unsplash.com/photo-1614200187524-dc4b892acf16?w=800',
      ],
      description: 'Porsche Cayenne certificado con garantía completa',
      location: 'La Romana',
      dealerName: 'Porsche RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 4)),
      doors: 4,
      seats: 5,
      engineSize: '3.0L',
      horsepower: 335,
      drivetrain: 'awd',
      features: const [
        'Sport Chrono',
        'Air Suspension',
        'Panoramic Roof',
        'BOSE Audio'
      ],
    ),
    VehicleModel(
      id: '5',
      make: 'Audi',
      model: 'e-tron GT',
      year: 2024,
      price: 105900,
      mileage: 50,
      condition: 'new',
      transmission: 'automatic',
      fuelType: 'electric',
      bodyType: 'sedan',
      color: 'Daytona Gray',
      images: const [
        'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800',
      ],
      description:
          'Gran turismo totalmente eléctrico con rendimiento impresionante',
      location: 'Santo Domingo',
      dealerName: 'Audi RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 5)),
      doors: 4,
      seats: 4,
      horsepower: 637,
      drivetrain: 'awd',
      features: const [
        'Quattro AWD',
        'Matrix LED',
        'Virtual Cockpit',
        'Bang & Olufsen'
      ],
    ),
    VehicleModel(
      id: '6',
      make: 'Ford',
      model: 'F-150',
      year: 2023,
      price: 52000,
      mileage: 15000,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'truck',
      color: 'Oxford White',
      images: const [
        'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800',
      ],
      description: 'F-150 confiable con paquete de remolque',
      location: 'Puerto Plata',
      dealerName: 'Ford RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 6)),
      doors: 4,
      seats: 5,
      engineSize: '5.0L',
      horsepower: 400,
      drivetrain: '4wd',
      features: const [
        'Towing Package',
        'FX4 Off-Road',
        'Leather',
        'Navigation'
      ],
    ),
    VehicleModel(
      id: '7',
      make: 'Lexus',
      model: 'RX 350',
      year: 2023,
      price: 49500,
      mileage: 18000,
      condition: 'certified',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'suv',
      color: 'Atomic Silver',
      images: const [
        'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
      ],
      description: 'SUV de lujo con confort premium',
      location: 'San Pedro',
      dealerName: 'Lexus RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 7)),
      doors: 4,
      seats: 7,
      engineSize: '3.5L',
      horsepower: 295,
      drivetrain: 'awd',
      features: const [
        'Mark Levinson Audio',
        'Panoramic View',
        'Heated Seats',
        'Navigation'
      ],
    ),
    VehicleModel(
      id: '8',
      make: 'Chevrolet',
      model: 'Corvette',
      year: 2024,
      price: 72500,
      mileage: 0,
      condition: 'new',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'coupe',
      color: 'Torch Red',
      images: const [
        'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800',
      ],
      description: 'Ícono deportivo americano con motor central',
      location: 'Bávaro',
      dealerName: 'Chevy Performance',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 8)),
      doors: 2,
      seats: 2,
      engineSize: '6.2L',
      horsepower: 495,
      drivetrain: 'rwd',
      features: const [
        'Z51 Performance',
        'Magnetic Ride',
        'Performance Exhaust',
        'GT2 Seats'
      ],
    ),
    VehicleModel(
      id: '9',
      make: 'Toyota',
      model: 'RAV4 Hybrid',
      year: 2023,
      price: 34500,
      mileage: 22000,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'hybrid',
      bodyType: 'suv',
      color: 'Blueprint',
      images: const [
        'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
      ],
      description: 'SUV híbrido eficiente con AWD',
      location: 'Barahona',
      dealerName: 'Toyota RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 9)),
      doors: 4,
      seats: 5,
      engineSize: '2.5L',
      horsepower: 219,
      drivetrain: 'awd',
      features: const [
        'Hybrid System',
        'Safety Sense',
        'Apple CarPlay',
        'Sunroof'
      ],
    ),
    VehicleModel(
      id: '10',
      make: 'Honda',
      model: 'Accord',
      year: 2024,
      price: 31000,
      mileage: 0,
      condition: 'new',
      transmission: 'automatic',
      fuelType: 'hybrid',
      bodyType: 'sedan',
      color: 'Platinum White',
      images: const [
        'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800',
      ],
      description: 'Accord Híbrido nuevo con tecnología avanzada',
      location: 'Naco',
      dealerName: 'Honda RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 10)),
      doors: 4,
      seats: 5,
      engineSize: '2.0L',
      horsepower: 204,
      drivetrain: 'fwd',
      features: const [
        'Honda Sensing',
        'Wireless CarPlay',
        'Sunroof',
        'Heated Seats'
      ],
    ),
    VehicleModel(
      id: '11',
      make: 'Mazda',
      model: 'CX-5',
      year: 2023,
      price: 29800,
      mileage: 16000,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'suv',
      color: 'Soul Red',
      images: const [
        'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800',
      ],
      description: 'SUV compacto elegante con sensación premium',
      location: 'Gascüe',
      dealerName: 'Mazda RD',
      isFeatured: true,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 11)),
      doors: 4,
      seats: 5,
      engineSize: '2.5L',
      horsepower: 187,
      drivetrain: 'awd',
      features: const ['i-Activsense', 'Bose Audio', 'Leather', 'Heated Seats'],
    ),

    // Additional vehicles for other sections (60+ more)
    // Daily Deals
    VehicleModel(
      id: '12',
      make: 'Hyundai',
      model: 'Elantra',
      year: 2022,
      price: 19500,
      mileage: 28000,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'sedan',
      color: 'Silver',
      images: const [
        'https://images.unsplash.com/photo-1617531653520-bd466e4337a9?w=800'
      ],
      description: 'Sedán económico y confiable',
      location: 'Higüey',
      dealerName: 'Hyundai RD',
      isFeatured: false,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 12)),
      doors: 4,
      seats: 5,
      engineSize: '2.0L',
      horsepower: 147,
      drivetrain: 'fwd',
      features: const ['Apple CarPlay', 'Rear Camera', 'Bluetooth'],
    ),
    // Add 60+ more vehicles here with variety in:
    // - Price ranges ($15k - $150k)
    // - Body types (sedan, suv, truck, coupe, hatchback, van)
    // - Fuel types (gasoline, diesel, electric, hybrid)
    // - Conditions (new, used, certified)
    // - Locations (various US cities)

    // For brevity, I'll add a few more key vehicles
    VehicleModel(
      id: '13',
      make: 'Nissan',
      model: 'Altima',
      year: 2023,
      price: 26500,
      mileage: 12000,
      condition: 'used',
      transmission: 'automatic',
      fuelType: 'gasoline',
      bodyType: 'sedan',
      color: 'Gun Metallic',
      images: const [
        'https://images.unsplash.com/photo-1610647752706-3bb12232b37a?w=800'
      ],
      description: 'Sedán mediano con excelente relación calidad-precio',
      location: 'La Vega',
      dealerName: 'Nissan RD',
      isFeatured: false,
      isVerified: true,
      createdAt: DateTime.now().subtract(const Duration(days: 13)),
      doors: 4,
      seats: 5,
      engineSize: '2.5L',
      horsepower: 188,
      drivetrain: 'fwd',
      features: const ['ProPILOT Assist', 'Remote Start', 'Heated Steering'],
    ),

    // Generate 58 more vehicles to complete 71 total
    ..._generateAdditionalVehicles(),
  ];

  /// Generate additional vehicles dynamically
  static List<VehicleModel> _generateAdditionalVehicles() {
    final vehicles = <VehicleModel>[];
    final makes = [
      'Toyota',
      'Honda',
      'Ford',
      'Chevrolet',
      'BMW',
      'Mercedes-Benz',
      'Audi',
      'Lexus',
      'Mazda',
      'Volkswagen'
    ];
    final models = ['Sedan', 'SUV', 'Coupe', 'Hatchback', 'Truck', 'Van'];
    final colors = ['Negro', 'Blanco', 'Plateado', 'Rojo', 'Azul', 'Gris'];
    final cities = [
      'Santo Domingo',
      'Santiago',
      'Punta Cana',
      'Puerto Plata',
      'La Romana',
      'San Cristóbal',
      'Bonao',
      'Moca',
    ];
    final bodyTypes = ['sedan', 'suv', 'coupe', 'truck', 'hatchback'];
    final fuelTypes = ['gasoline', 'diesel', 'hybrid', 'electric'];
    final conditions = ['new', 'used', 'certified'];

    for (var i = 0; i < 58; i++) {
      final makeIndex = i % makes.length;
      final bodyTypeIndex = i % bodyTypes.length;
      final isFeatured = i < 10; // First 10 additional are featured
      final isElectric = fuelTypes[i % fuelTypes.length] == 'electric';
      final isHybrid = fuelTypes[i % fuelTypes.length] == 'hybrid';
      final isSUV = bodyTypes[bodyTypeIndex] == 'suv';
      final price = 20000.0 + (i * 1500);

      vehicles.add(VehicleModel(
        id: (14 + i).toString(),
        make: makes[makeIndex],
        model: '${models[i % models.length]} ${2020 + (i % 5)}',
        year: 2020 + (i % 5),
        price: price,
        mileage: i < 20 ? (i * 500) : (5000 + (i * 1000)),
        condition: conditions[i % conditions.length],
        transmission: 'automatic',
        fuelType: fuelTypes[i % fuelTypes.length],
        bodyType: bodyTypes[bodyTypeIndex],
        color: colors[i % colors.length],
        images: [
          'https://images.unsplash.com/photo-${1617531653520 + i}?w=800',
        ],
        description:
            '${makes[makeIndex]} ${models[i % models.length]} en excelente estado',
        location: cities[i % cities.length],
        dealerName: '${makes[makeIndex]} RD',
        isFeatured: isFeatured,
        isVerified: i % 3 == 0,
        createdAt: DateTime.now().subtract(Duration(days: 14 + i)),
        doors: bodyTypes[bodyTypeIndex] == 'truck' ? 2 : 4,
        seats: bodyTypes[bodyTypeIndex] == 'truck' ? 2 : 5,
        engineSize: '${2.0 + (i % 3)}.0L',
        horsepower: 150 + (i * 5),
        drivetrain:
            isSUV || bodyTypes[bodyTypeIndex] == 'truck' ? 'awd' : 'fwd',
        features: [
          'Bluetooth',
          'Backup Camera',
          if (isElectric || isHybrid) 'Eco Mode',
          if (price > 40000) 'Premium Audio',
          if (isFeatured) 'Navigation System',
        ],
      ));
    }

    return vehicles;
  }
}
