namespace AlertService.Domain.Entities;

public class PriceAlert
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid VehicleId { get; private set; }
    public decimal TargetPrice { get; private set; }
    public AlertCondition Condition { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsTriggered { get; private set; }
    public DateTime? TriggeredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // For EF Core
    private PriceAlert() { }

    public PriceAlert(Guid userId, Guid vehicleId, decimal targetPrice, AlertCondition condition)
    {
        if (targetPrice <= 0)
            throw new ArgumentException("Target price must be greater than zero", nameof(targetPrice));

        Id = Guid.NewGuid();
        UserId = userId;
        VehicleId = vehicleId;
        TargetPrice = targetPrice;
        Condition = condition;
        IsActive = true;
        IsTriggered = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTargetPrice(decimal newTargetPrice)
    {
        if (newTargetPrice <= 0)
            throw new ArgumentException("Target price must be greater than zero", nameof(newTargetPrice));

        TargetPrice = newTargetPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Trigger()
    {
        IsTriggered = true;
        TriggeredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reset()
    {
        IsTriggered = false;
        TriggeredAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ShouldTrigger(decimal currentPrice)
    {
        if (!IsActive || IsTriggered)
            return false;

        return Condition switch
        {
            AlertCondition.LessThanOrEqual => currentPrice <= TargetPrice,
            AlertCondition.GreaterThanOrEqual => currentPrice >= TargetPrice,
            _ => false
        };
    }
}

public enum AlertCondition
{
    LessThanOrEqual = 0,    // Alerta cuando el precio baje a X o menos
    GreaterThanOrEqual = 1  // Alerta cuando el precio suba a X o más
}
