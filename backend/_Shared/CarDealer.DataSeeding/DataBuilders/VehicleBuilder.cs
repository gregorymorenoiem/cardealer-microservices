using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarDealer.DataSeeding.DataBuilders;

/// <summary>
/// Fluent builder para crear vehículos realistas
/// Uso: new VehicleBuilder().WithMake("Toyota").WithModel("Corolla").WithPrice(15000).Build()
/// </summary>
public class VehicleBuilder
{
    private string _make = "Toyota";
    private string _model = "Corolla";
    private int _year = 2020;
    private decimal _price = 15000;
    private int _mileage = 50000;
    private string _condition = "Used";
    private string _bodyStyle = "Sedan";
    private string _fuelType = "Gasoline";
    private string _transmission = "Automatic";
    private string _exteriorColor = "Silver";
    private string _interiorColor = "Black";
    private int _doors = 4;
    private int _seats = 5;
    private string _vinNumber = "";
    private Guid _dealerId = Guid.Empty;
    private bool _isFeatured = false;
    private string _description = "";

    private static readonly Faker _faker = new("es_MX");

    // Datos realistas de vehículos
    private static readonly Dictionary<string, string[]> MakeModels = new()
    {
        { "Toyota", new[] { "Corolla", "Camry", "Hilux", "Land Cruiser", "RAV4", "Yaris", "Prius", "FJ Cruiser" } },
        { "Honda", new[] { "Civic", "Accord", "CR-V", "Odyssey", "Pilot", "HR-V", "Ridgeline" } },
        { "Nissan", new[] { "Sentra", "Altima", "Pathfinder", "Qashqai", "Frontier", "Versa", "X-Trail" } },
        { "Hyundai", new[] { "Elantra", "Sonata", "Tucson", "Santa Fe", "Accent", "Venue", "Kona" } },
        { "Ford", new[] { "Focus", "Fusion", "F-150", "Ecosport", "Ranger", "Edge", "Escape" } },
        { "Mazda", new[] { "Mazda3", "Mazda6", "CX-5", "CX-3", "Mazda2", "BT-50" } },
        { "Chevrolet", new[] { "Spark", "Cruze", "Silverado", "Equinox", "Traverse", "Malibu" } },
        { "Volkswagen", new[] { "Jetta", "Passat", "Tiguan", "Polo", "Amarok" } },
        { "Mitsubishi", new[] { "Lancer", "Outlander", "L200", "Pajero", "Mirage" } },
        { "Kia", new[] { "Cerato", "Sportage", "Sorento", "Forte", "Picanto" } }
    };

    private static readonly string[] BodyStyles = 
    { 
        "Sedan", "SUV", "Hatchback", "Truck", "Coupe", "Wagon", "Van", "Convertible", "Crossover" 
    };

    private static readonly string[] FuelTypes = 
    { 
        "Gasoline", "Diesel", "Hybrid", "Electric", "Natural Gas" 
    };

    public VehicleBuilder WithMake(string make)
    {
        if (MakeModels.ContainsKey(make))
            _make = make;
        return this;
    }

    public VehicleBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }

    public VehicleBuilder WithYear(int year)
    {
        _year = year;
        return this;
    }

    public VehicleBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public VehicleBuilder WithMileage(int mileage)
    {
        _mileage = mileage;
        return this;
    }

    public VehicleBuilder AsNew()
    {
        _condition = "New";
        _mileage = _faker.Random.Int(0, 100);
        _price = _price * 1.2m; // 20% más caro si es nuevo
        return this;
    }

    public VehicleBuilder AsUsed()
    {
        _condition = "Used";
        _mileage = _faker.Random.Int(10_000, 200_000);
        return this;
    }

    public VehicleBuilder AsCertified()
    {
        _condition = "Certified";
        _mileage = _faker.Random.Int(5_000, 100_000);
        _price = _price * 1.1m; // 10% más caro si es certificado
        return this;
    }

    public VehicleBuilder WithBodyStyle(string bodyStyle)
    {
        _bodyStyle = bodyStyle;
        return this;
    }

    public VehicleBuilder WithFuelType(string fuelType)
    {
        _fuelType = fuelType;
        return this;
    }

    public VehicleBuilder WithTransmission(string transmission)
    {
        _transmission = transmission;
        return this;
    }

    public VehicleBuilder WithColor(string color)
    {
        _exteriorColor = color;
        return this;
    }

    public VehicleBuilder WithDoors(int doors)
    {
        _doors = doors;
        return this;
    }

    public VehicleBuilder WithSeats(int seats)
    {
        _seats = seats;
        return this;
    }

    public VehicleBuilder ForDealer(Guid dealerId)
    {
        _dealerId = dealerId;
        return this;
    }

    public VehicleBuilder AsFeatured()
    {
        _isFeatured = true;
        _price = _price * 0.95m; // 5% descuento en destacados
        return this;
    }

    public VehicleDto Build()
    {
        // Generar VIN único
        if (string.IsNullOrEmpty(_vinNumber))
            _vinNumber = GenerateVin();

        // Generar descripción si no la hay
        if (string.IsNullOrEmpty(_description))
            _description = GenerateDescription();

        // Color interior aleatorio si no se especifica
        if (_interiorColor == "Black")
            _interiorColor = _faker.PickRandom(new[] { "Black", "Beige", "Gray", "Brown", "Tan" });

        var title = $"{_year} {_make} {_model}";

        return new VehicleDto
        {
            Id = Guid.NewGuid(),
            Title = title,
            Make = _make,
            Model = _model,
            Year = _year,
            Price = _price,
            Mileage = _mileage,
            Condition = _condition,
            BodyStyle = _bodyStyle,
            FuelType = _fuelType,
            Transmission = _transmission,
            ExteriorColor = _exteriorColor,
            InteriorColor = _interiorColor,
            Doors = _doors,
            Seats = _seats,
            VinNumber = _vinNumber,
            DealerId = _dealerId,
            IsFeatured = _isFeatured,
            Description = _description,
            CreatedAt = DateTime.UtcNow,
            Status = "Active"
        };
    }

    /// <summary>
    /// Genera múltiples vehículos distribuidos entre dealers
    /// </summary>
    public static List<VehicleDto> GenerateBatch(
        int count,
        List<Guid> dealerIds,
        int vehiclesPerDealer = 5)
    {
        var vehicles = new List<VehicleDto>();
        var makes = MakeModels.Keys.ToList();

        for (int i = 0; i < count; i++)
        {
            var make = makes[i % makes.Count]; // Distribuir marcas
            var model = _faker.PickRandom(MakeModels[make]);
            var dealerId = dealerIds[i % dealerIds.Count];

            var builder = new VehicleBuilder()
                .WithMake(make)
                .WithModel(model)
                .WithYear(_faker.Random.Int(2010, 2024))
                .WithPrice((decimal)_faker.Random.Int(10_000, 250_000) * 100)
                .WithBodyStyle(_faker.PickRandom(BodyStyles))
                .WithFuelType(_faker.PickRandom(FuelTypes))
                .WithColor(_faker.Color.ColorName())
                .ForDealer(dealerId);

            // 60% usado, 30% nuevo, 10% certificado
            var conditionRandom = _faker.Random.Int(0, 9);
            if (conditionRandom < 3)
                builder = builder.AsNew();
            else if (conditionRandom < 9)
                builder = builder.AsUsed();
            else
                builder = builder.AsCertified();

            // 20% destacados
            if (_faker.Random.Bool(0.2f))
                builder = builder.AsFeatured();

            vehicles.Add(builder.Build());
        }

        return vehicles;
    }

    // HELPERS

    private static string GenerateVin()
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        var vin = "3G";
        var random = new Random();

        for (int i = 0; i < 15; i++)
            vin += chars[random.Next(chars.Length)];

        return vin;
    }

    private string GenerateDescription()
    {
        var features = new List<string>
        {
            "Aire acondicionado",
            "Dirección asistida",
            "Cierre centralizado",
            "Elevalunas eléctricos",
            "Espejos eléctricos",
            "Control de tracción",
            "ABS",
            "Airbags",
            "Bluetooth",
            "Pantalla táctil",
            "Sensor de estacionamiento",
            "Cámara de reversa",
            "Cruise control",
            "Techo corredizo"
        };

        var selectedFeatures = _faker.Random.ListItems(features, _faker.Random.Int(3, 8));

        var descriptions = new[]
        {
            $"Excelente estado. Características: {string.Join(", ", selectedFeatures)}. Documentación completa.",
            $"Vehículo en perfectas condiciones. Incluye {string.Join(", ", selectedFeatures)}. Mantenimiento al día.",
            $"Oportunidad única. {string.Join(", ", selectedFeatures)}. Preço muy competitivo.",
            $"Automóvil bien conservado con {string.Join(", ", selectedFeatures)}. Ideal para familias.",
            $"Impecable, como nuevo. Cuenta con {string.Join(", ", selectedFeatures)}. Inversión segura."
        };

        return _faker.PickRandom(descriptions);
    }
}

public class VehicleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Condition { get; set; } = "Used";
    public string BodyStyle { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = "Automatic";
    public string ExteriorColor { get; set; } = string.Empty;
    public string InteriorColor { get; set; } = string.Empty;
    public int Doors { get; set; } = 4;
    public int Seats { get; set; } = 5;
    public string VinNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DealerId { get; set; }
    public bool IsFeatured { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}
