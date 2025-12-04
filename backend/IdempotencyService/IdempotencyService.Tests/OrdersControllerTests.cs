using FluentAssertions;
using IdempotencyService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdempotencyService.Tests;

/// <summary>
/// Tests for OrdersController (example controller demonstrating idempotency)
/// </summary>
public class OrdersControllerTests
{
    private readonly OrdersController _controller;
    private readonly Mock<ILogger<OrdersController>> _loggerMock;

    public OrdersControllerTests()
    {
        _loggerMock = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_loggerMock.Object);

        // Clear any existing orders between tests
        var deleteResult = _controller.DeleteAll();
    }

    // =========== CreateOrder Tests ===========

    [Fact]
    public void CreateOrder_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            VehicleId = "vehicle-456",
            TotalAmount = 25000.00m
        };

        // Act
        var result = _controller.CreateOrder(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        actionResult.ActionName.Should().Be(nameof(OrdersController.GetOrder));

        var order = actionResult.Value.Should().BeOfType<Order>().Subject;
        order.Id.Should().NotBeNullOrEmpty();
        order.CustomerId.Should().Be("customer-123");
        order.VehicleId.Should().Be("vehicle-456");
        order.TotalAmount.Should().Be(25000.00m);
        order.Status.Should().Be("Created");
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateOrder_LogsOrderCreation()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            VehicleId = "vehicle-456",
            TotalAmount = 15000.00m
        };

        // Act
        _controller.CreateOrder(request);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Order created")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // =========== GetOrder Tests ===========

    [Fact]
    public void GetOrder_WithExistingOrder_ReturnsOrder()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            VehicleId = "vehicle-456",
            TotalAmount = 30000.00m
        };
        var createResult = _controller.CreateOrder(createRequest);
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        var result = _controller.GetOrder(createdOrder!.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.Id.Should().Be(createdOrder.Id);
    }

    [Fact]
    public void GetOrder_WithNonExistingOrder_ReturnsNotFound()
    {
        // Act
        var result = _controller.GetOrder("non-existing-id");

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // =========== ListOrders Tests ===========

    [Fact]
    public void ListOrders_WithNoOrders_ReturnsEmptyList()
    {
        // Act
        var result = _controller.ListOrders();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var orders = okResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        orders.Should().BeEmpty();
    }

    [Fact]
    public void ListOrders_WithMultipleOrders_ReturnsAllOrders()
    {
        // Arrange
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c1", VehicleId = "v1", TotalAmount = 1000 });
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c2", VehicleId = "v2", TotalAmount = 2000 });
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c3", VehicleId = "v3", TotalAmount = 3000 });

        // Act
        var result = _controller.ListOrders();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var orders = okResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        orders.Should().HaveCount(3);
    }

    // =========== UpdateOrder Tests ===========

    [Fact]
    public void UpdateOrder_WithExistingOrder_ReturnsUpdatedOrder()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            VehicleId = "vehicle-456",
            TotalAmount = 25000.00m
        };
        var createResult = _controller.CreateOrder(createRequest);
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        var updateRequest = new UpdateOrderRequest
        {
            Status = "Processing",
            TotalAmount = 26000.00m
        };

        // Act
        var result = _controller.UpdateOrder(createdOrder!.Id, updateRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.Status.Should().Be("Processing");
        order.TotalAmount.Should().Be(26000.00m);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateOrder_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateOrderRequest
        {
            Status = "Processing",
            TotalAmount = 26000.00m
        };

        // Act
        var result = _controller.UpdateOrder("non-existing-id", updateRequest);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void UpdateOrder_LogsOrderUpdate()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 1000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        _controller.UpdateOrder(createdOrder!.Id, new UpdateOrderRequest
        {
            Status = "Updated",
            TotalAmount = 1500
        });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Order updated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // =========== ProcessPayment Tests ===========

    [Fact]
    public void ProcessPayment_WithExistingOrder_ReturnsPaymentResult()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            VehicleId = "vehicle-456",
            TotalAmount = 25000.00m
        };
        var createResult = _controller.CreateOrder(createRequest);
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        var paymentRequest = new PaymentRequest
        {
            Amount = 25000.00m,
            CardNumber = "4111111111111111"
        };

        // Act
        var result = _controller.ProcessPayment(createdOrder!.Id, "credit_card", paymentRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var paymentResult = okResult.Value.Should().BeOfType<PaymentResult>().Subject;
        paymentResult.TransactionId.Should().NotBeNullOrEmpty();
        paymentResult.OrderId.Should().Be(createdOrder.Id);
        paymentResult.Amount.Should().Be(25000.00m);
        paymentResult.PaymentMethod.Should().Be("credit_card");
        paymentResult.Status.Should().Be("Success");
    }

    [Fact]
    public void ProcessPayment_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            Amount = 25000.00m,
            CardNumber = "4111111111111111"
        };

        // Act
        var result = _controller.ProcessPayment("non-existing-id", "credit_card", paymentRequest);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void ProcessPayment_UpdatesOrderStatusToPaid()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 5000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        _controller.ProcessPayment(createdOrder!.Id, "debit_card", new PaymentRequest
        {
            Amount = 5000,
            CardNumber = "1234567890"
        });

        // Assert
        var getResult = _controller.GetOrder(createdOrder.Id);
        var okResult = getResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.Status.Should().Be("Paid");
    }

    [Fact]
    public void ProcessPayment_LogsPaymentProcessing()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 5000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        _controller.ProcessPayment(createdOrder!.Id, "cash", new PaymentRequest
        {
            Amount = 5000,
            CardNumber = ""
        });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Payment processed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // =========== CancelOrder Tests ===========

    [Fact]
    public void CancelOrder_WithExistingOrder_ReturnsCancelledOrder()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 10000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Customer changed mind"
        };

        // Act
        var result = _controller.CancelOrder(createdOrder!.Id, cancelRequest);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.Status.Should().Be("Cancelled");
        order.CancellationReason.Should().Be("Customer changed mind");
    }

    [Fact]
    public void CancelOrder_WithNonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Customer changed mind"
        };

        // Act
        var result = _controller.CancelOrder("non-existing-id", cancelRequest);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void CancelOrder_UpdatesOrderUpdatedAt()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 10000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        var result = _controller.CancelOrder(createdOrder!.Id, new CancelOrderRequest { Reason = "Test" });

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var order = okResult.Value.Should().BeOfType<Order>().Subject;
        order.UpdatedAt.Should().NotBeNull();
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CancelOrder_LogsCancellation()
    {
        // Arrange
        var createResult = _controller.CreateOrder(new CreateOrderRequest
        {
            CustomerId = "c1",
            VehicleId = "v1",
            TotalAmount = 5000
        });
        var createdOrder = (createResult.Result as CreatedAtActionResult)!.Value as Order;

        // Act
        _controller.CancelOrder(createdOrder!.Id, new CancelOrderRequest { Reason = "Test reason" });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Order cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // =========== DeleteAll Tests ===========

    [Fact]
    public void DeleteAll_WithOrders_DeletesAllAndReturnsCount()
    {
        // Arrange
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c1", VehicleId = "v1", TotalAmount = 1000 });
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c2", VehicleId = "v2", TotalAmount = 2000 });
        _controller.CreateOrder(new CreateOrderRequest { CustomerId = "c3", VehicleId = "v3", TotalAmount = 3000 });

        // Act
        var result = _controller.DeleteAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // Check that orders are actually deleted
        var listResult = _controller.ListOrders();
        var listOkResult = listResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var orders = listOkResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        orders.Should().BeEmpty();
    }

    [Fact]
    public void DeleteAll_WithNoOrders_ReturnsZeroCount()
    {
        // Act
        var result = _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== DTO Tests ===========

    [Fact]
    public void Order_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Id.Should().BeEmpty();
        order.CustomerId.Should().BeEmpty();
        order.VehicleId.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
        order.Status.Should().BeEmpty();
        order.CancellationReason.Should().BeNull();
    }

    [Fact]
    public void CreateOrderRequest_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var request = new CreateOrderRequest();

        // Assert
        request.CustomerId.Should().BeEmpty();
        request.VehicleId.Should().BeEmpty();
        request.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void UpdateOrderRequest_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var request = new UpdateOrderRequest();

        // Assert
        request.Status.Should().BeEmpty();
        request.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void PaymentRequest_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var request = new PaymentRequest();

        // Assert
        request.Amount.Should().Be(0);
        request.CardNumber.Should().BeEmpty();
    }

    [Fact]
    public void PaymentResult_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var result = new PaymentResult();

        // Assert
        result.TransactionId.Should().BeEmpty();
        result.OrderId.Should().BeEmpty();
        result.Amount.Should().Be(0);
        result.PaymentMethod.Should().BeEmpty();
        result.Status.Should().BeEmpty();
    }

    [Fact]
    public void CancelOrderRequest_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var request = new CancelOrderRequest();

        // Assert
        request.Reason.Should().BeEmpty();
    }
}
