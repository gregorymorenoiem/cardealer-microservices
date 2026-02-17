using VehiclesRentService.Domain.Entities;

namespace VehiclesRentService.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id);
    Task<Vehicle?> GetByVINAsync(string vin);
    Task<IEnumerable<Vehicle>> GetAllAsync(int skip = 0, int take = 100);
    Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchParameters parameters);
    Task<IEnumerable<Vehicle>> GetBySellerAsync(Guid sellerId);
    Task<IEnumerable<Vehicle>> GetByDealerAsync(Guid dealerId);
    Task<IEnumerable<Vehicle>> GetFeaturedAsync(int take = 10);
    Task<Vehicle> CreateAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountAsync(VehicleSearchParameters? parameters = null);
}

public class VehicleSearchParameters
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MaxMileage { get; set; }
    public VehicleType? VehicleType { get; set; }
    public BodyStyle? BodyStyle { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public Entities.DriveType? DriveType { get; set; }
    public VehicleCondition? Condition { get; set; }
    public string? ExteriorColor { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public bool? IsCertified { get; set; }
    public bool? HasCleanTitle { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
