using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarDealer.DataSeeding.DataBuilders;

/// <summary>
/// Fluent builder para crear dealers realistas con Bogus
/// Uso: new DealerBuilder().WithName("Premium Motors").AsChain().Build()
/// </summary>
public class DealerBuilder
{
    private string _businessName = "Unnamed Dealer";
    private string _rnc = "";
    private string _dealerType = "Independent";
    private string _email = "";
    private string _phone = "";
    private string _website = "";
    private string _city = "Santo Domingo";
    private string _address = "";
    private int _employeeCount = 5;
    private DateTime _establishedDate = DateTime.UtcNow.AddYears(-5);
    private string _status = "Active";
    private string _verificationStatus = "Verified";
    private List<DealerLocationDto> _locations = new();

    private static readonly Faker _faker = new("es_MX");

    public DealerBuilder WithName(string name)
    {
        _businessName = name;
        return this;
    }

    public DealerBuilder WithRnc(string rnc)
    {
        _rnc = rnc;
        return this;
    }

    public DealerBuilder AsIndependent()
    {
        _dealerType = "Independent";
        _employeeCount = _faker.Random.Int(1, 10);
        return this;
    }

    public DealerBuilder AsChain()
    {
        _dealerType = "Chain";
        _employeeCount = _faker.Random.Int(50, 200);
        _locations.AddRange(GenerateLocations(3));
        return this;
    }

    public DealerBuilder AsMultipleStore()
    {
        _dealerType = "MultipleStore";
        _employeeCount = _faker.Random.Int(20, 100);
        _locations.AddRange(GenerateLocations(2));
        return this;
    }

    public DealerBuilder AsFranchise()
    {
        _dealerType = "Franchise";
        _employeeCount = _faker.Random.Int(10, 50);
        _locations.AddRange(GenerateLocations(2));
        return this;
    }

    public DealerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public DealerBuilder WithPhone(string phone)
    {
        _phone = phone;
        return this;
    }

    public DealerBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public DealerBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public DealerBuilder AsVerified()
    {
        _verificationStatus = "Verified";
        _status = "Active";
        return this;
    }

    public DealerBuilder AsPending()
    {
        _verificationStatus = "UnderReview";
        _status = "Pending";
        return this;
    }

    public DealerBuilder WithEstablishedDate(DateTime date)
    {
        _establishedDate = date;
        return this;
    }

    public DealerDto Build()
    {
        // Generar valores por defecto si no están seteados
        if (string.IsNullOrEmpty(_rnc))
            _rnc = _faker.Random.ReplaceNumbers("###########");

        if (string.IsNullOrEmpty(_email))
            _email = _faker.Internet.Email(_businessName.Replace(" ", "").ToLower() + ".do");

        if (string.IsNullOrEmpty(_phone))
            _phone = _faker.Phone.PhoneNumber("809-####-####");

        if (string.IsNullOrEmpty(_website))
            _website = _faker.Internet.Url();

        if (string.IsNullOrEmpty(_address))
            _address = _faker.Address.FullAddress();

        if (_locations.Count == 0)
            _locations.Add(CreateLocation(_city, true));

        return new DealerDto
        {
            Id = Guid.NewGuid(),
            BusinessName = _businessName,
            Rnc = _rnc,
            DealerType = _dealerType,
            Email = _email,
            Phone = _phone,
            Website = _website,
            City = _city,
            Address = _address,
            EmployeeCount = _employeeCount,
            EstablishedDate = _establishedDate,
            Status = _status,
            VerificationStatus = _verificationStatus,
            Description = GenerateDescription(),
            Locations = _locations,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Genera múltiples dealers en una sola llamada
    /// </summary>
    public static List<DealerDto> GenerateBatch(int count, string type = "Mixed")
    {
        var dealers = new List<DealerDto>();
        var dealerTypes = new[] { "Independent", "Chain", "MultipleStore", "Franchise" };

        for (int i = 0; i < count; i++)
        {
            var dealerType = type == "Mixed"
                ? dealerTypes[_faker.Random.Int(0, dealerTypes.Length - 1)]
                : type;

            var builder = new DealerBuilder()
                .WithName(_faker.Company.CompanyName() + " " + _faker.Company.Suffix());

            switch (dealerType)
            {
                case "Chain":
                    builder = builder.AsChain();
                    break;
                case "MultipleStore":
                    builder = builder.AsMultipleStore();
                    break;
                case "Franchise":
                    builder = builder.AsFranchise();
                    break;
                default:
                    builder = builder.AsIndependent();
                    break;
            }

            if (_faker.Random.Bool(0.7f)) // 70% verified
                builder = builder.AsVerified();
            else
                builder = builder.AsPending();

            var cities = new[] { "Santo Domingo", "Santiago", "La Romana", "San Francisco de Macorís", "Sosúa" };
            builder = builder.WithCity(_faker.PickRandom(cities));

            dealers.Add(builder.Build());
        }

        return dealers;
    }

    // HELPERS

    private List<DealerLocationDto> GenerateLocations(int count)
    {
        var locations = new List<DealerLocationDto>();
        var cities = new[] { "Santo Domingo", "Santiago", "La Romana", "San Francisco de Macorís", "Sosúa" };

        for (int i = 0; i < count; i++)
        {
            locations.Add(CreateLocation(_faker.PickRandom(cities), i == 0));
        }

        return locations;
    }

    private DealerLocationDto CreateLocation(string city, bool isPrimary = false)
    {
        return new DealerLocationDto
        {
            Id = Guid.NewGuid(),
            Name = isPrimary ? "Sede Principal" : $"Sucursal {city}",
            Address = _faker.Address.FullAddress(),
            City = city,
            Phone = _faker.Phone.PhoneNumber("809-####-####"),
            Email = _faker.Internet.Email(),
            IsPrimary = isPrimary,
            Latitude = double.Parse(_faker.Address.Latitude().ToString()),
            Longitude = double.Parse(_faker.Address.Longitude().ToString())
        };
    }

    private string GenerateDescription()
    {
        var templates = new[]
        {
            "Concesionario especializado en vehículos de calidad con garantía extendida.",
            "Automotriz con más de 10 años en el mercado dominicano.",
            "Distribuidor oficial de marcas reconocidas internacionalmente.",
            "Servicio integral: ventas, financiamiento y mantenimiento.",
            "Inventario actualizado con modelos nuevos y usados certificados.",
        };

        return _faker.PickRandom(templates);
    }
}

// DTOs para seeding
public class DealerDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Rnc { get; set; } = string.Empty;
    public string DealerType { get; set; } = "Independent";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Description { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string VerificationStatus { get; set; } = "NotVerified";
    public List<DealerLocationDto> Locations { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class DealerLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
