using IdempotencyService.Core.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace IdempotencyService.Api.Controllers;

/// <summary>
/// Example controller demonstrating idempotency usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Idempotent(RequireKey = false)] // Apply to all actions by default
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private static readonly List<Order> _orders = new();

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order (idempotent with required key)
    /// </summary>
    [HttpPost]
    [Idempotent(RequireKey = true, TtlSeconds = 3600)]
    public ActionResult<Order> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            TotalAmount = request.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            Status = "Created"
        };

        _orders.Add(order);

        _logger.LogInformation("Order created: {OrderId}", order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Updates an order (idempotent with custom prefix)
    /// </summary>
    [HttpPut("{id}")]
    [Idempotent(RequireKey = true, KeyPrefix = "order-update")]
    public ActionResult<Order> UpdateOrder(string id, [FromBody] UpdateOrderRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = request.Status;
        order.TotalAmount = request.TotalAmount;
        order.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Order updated: {OrderId}", order.Id);

        return Ok(order);
    }

    /// <summary>
    /// Processes payment for an order (idempotent, includes query in hash)
    /// </summary>
    [HttpPost("{id}/payment")]
    [Idempotent(RequireKey = true, IncludeQueryInHash = true, KeyPrefix = "payment")]
    public ActionResult<PaymentResult> ProcessPayment(
        string id,
        [FromQuery] string paymentMethod,
        [FromBody] PaymentRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        // Simulate payment processing
        var result = new PaymentResult
        {
            TransactionId = Guid.NewGuid().ToString(),
            OrderId = id,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            Status = "Success",
            ProcessedAt = DateTime.UtcNow
        };

        order.Status = "Paid";
        order.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Payment processed for order {OrderId}: {TransactionId}",
            id, result.TransactionId);

        return Ok(result);
    }

    /// <summary>
    /// Gets an order (not idempotent - read operation)
    /// </summary>
    [HttpGet("{id}")]
    [SkipIdempotency]
    public ActionResult<Order> GetOrder(string id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    /// <summary>
    /// Lists all orders (not idempotent - read operation)
    /// </summary>
    [HttpGet]
    [SkipIdempotency]
    public ActionResult<IEnumerable<Order>> ListOrders()
    {
        return Ok(_orders);
    }

    /// <summary>
    /// Cancels an order (idempotent with body hash only)
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Idempotent(RequireKey = true, IncludeBodyInHash = false, KeyPrefix = "cancel")]
    public ActionResult<Order> CancelOrder(string id, [FromBody] CancelOrderRequest request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "Cancelled";
        order.CancellationReason = request.Reason;
        order.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Order cancelled: {OrderId}, Reason: {Reason}",
            order.Id, request.Reason);

        return Ok(order);
    }

    /// <summary>
    /// Deletes all orders (not idempotent, requires explicit call)
    /// </summary>
    [HttpDelete("all")]
    [SkipIdempotency]
    public IActionResult DeleteAll()
    {
        var count = _orders.Count;
        _orders.Clear();
        return Ok(new { deletedCount = count });
    }
}

// DTOs
public class Order
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class UpdateOrderRequest
{
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
}

public class PaymentResult
{
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}
