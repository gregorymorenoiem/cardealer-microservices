using CarDealer.Shared.Sagas.Configuration;
using CarDealer.Shared.Sagas.Contracts;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Sagas;

public class SagaOptionsTests
{
    // ── SectionName ──────────────────────────────────────────────────
    [Fact]
    public void SectionName_ShouldBe_Sagas()
    {
        SagaOptions.SectionName.Should().Be("Sagas");
    }

    // ── Top-level defaults ───────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var opts = new SagaOptions();

        opts.Enabled.Should().BeTrue();
        opts.RabbitMq.Should().NotBeNull();
        opts.Repository.Should().NotBeNull();
        opts.Retry.Should().NotBeNull();
        opts.Outbox.Should().NotBeNull();
    }

    // ── RabbitMqOptions defaults ─────────────────────────────────────
    [Fact]
    public void RabbitMqOptions_DefaultValues_ShouldBeCorrect()
    {
        var rmq = new RabbitMqOptions();

        rmq.Host.Should().Be("localhost");
        rmq.Port.Should().Be(5672);
        rmq.VirtualHost.Should().Be("/");
        rmq.Username.Should().Be("guest");
        rmq.Password.Should().Be("guest");
        rmq.QueuePrefix.Should().Be("cardealer");
    }

    [Fact]
    public void RabbitMqOptions_ConnectionString_ShouldBeComputed()
    {
        var rmq = new RabbitMqOptions();
        rmq.ConnectionString.Should().Be("amqp://guest:guest@localhost:5672/");

        rmq.Host = "broker.example.com";
        rmq.Port = 5673;
        rmq.Username = "admin";
        rmq.Password = "secret";
        rmq.VirtualHost = "/prod";
        rmq.ConnectionString.Should().Be("amqp://admin:secret@broker.example.com:5673/prod");
    }

    // ── SagaRepositoryOptions defaults ───────────────────────────────
    [Fact]
    public void SagaRepositoryOptions_DefaultValues_ShouldBeCorrect()
    {
        var repo = new SagaRepositoryOptions();

        repo.Type.Should().Be(SagaRepositoryType.EntityFramework);
        repo.RedisConnectionString.Should().BeNull();
        repo.RedisKeyPrefix.Should().Be("saga:");
        repo.RedisExpirationMinutes.Should().Be(60 * 24);
    }

    // ── SagaRetryOptions defaults ────────────────────────────────────
    [Fact]
    public void SagaRetryOptions_DefaultValues_ShouldBeCorrect()
    {
        var retry = new SagaRetryOptions();

        retry.MaxRetries.Should().Be(3);
        retry.IntervalSeconds.Should().Be(5);
        retry.UseExponentialBackoff.Should().BeTrue();
    }

    // ── OutboxOptions defaults ───────────────────────────────────────
    [Fact]
    public void OutboxOptions_DefaultValues_ShouldBeCorrect()
    {
        var outbox = new OutboxOptions();

        outbox.Enabled.Should().BeTrue();
        outbox.CleanupIntervalSeconds.Should().Be(30);
        outbox.DeliveredMessageTtlHours.Should().Be(24);
        outbox.BatchSize.Should().Be(100);
    }

    // ── SagaRepositoryType enum values ───────────────────────────────
    [Fact]
    public void SagaRepositoryType_ShouldHaveExpectedValues()
    {
        Enum.GetValues<SagaRepositoryType>().Should().HaveCount(3);
        SagaRepositoryType.InMemory.Should().BeDefined();
        SagaRepositoryType.EntityFramework.Should().BeDefined();
        SagaRepositoryType.Redis.Should().BeDefined();
    }
}

public class SagaContractsTests
{
    // ── SagaFaulted record defaults ──────────────────────────────────
    [Fact]
    public void SagaFaulted_DefaultValues_ShouldBeCorrect()
    {
        var fault = new SagaFaulted();

        fault.CorrelationId.Should().Be(Guid.Empty);
        fault.ErrorCode.Should().Be("SAGA_FAULT");
        fault.ErrorMessage.Should().Be(string.Empty);
        fault.ErrorDetails.Should().BeNull();
        fault.SagaType.Should().Be(string.Empty);
        fault.CurrentState.Should().Be(string.Empty);
        fault.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // ── SubmitOrder record ───────────────────────────────────────────
    [Fact]
    public void SubmitOrder_ShouldHaveDefaultsAndAcceptValues()
    {
        var orderId = Guid.NewGuid();
        var cmd = new SubmitOrder
        {
            CorrelationId = orderId,
            CustomerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Amount = 25000m,
            PaymentMethod = "CreditCard"
        };

        cmd.CorrelationId.Should().Be(orderId);
        cmd.Amount.Should().Be(25000m);
        cmd.PaymentMethod.Should().Be("CreditCard");
        cmd.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // ── VehicleReservationFailed record defaults ─────────────────────
    [Fact]
    public void VehicleReservationFailed_ShouldHaveCorrectDefaults()
    {
        var evt = new VehicleReservationFailed();

        evt.ErrorCode.Should().Be("VEHICLE_RESERVATION_FAILED");
        evt.VehicleId.Should().Be(Guid.Empty);
    }

    // ── PaymentFailed record defaults ────────────────────────────────
    [Fact]
    public void PaymentFailed_ShouldHaveCorrectDefaults()
    {
        var evt = new PaymentFailed();

        evt.ErrorCode.Should().Be("PAYMENT_FAILED");
        evt.ErrorMessage.Should().Be(string.Empty);
        evt.ErrorDetails.Should().BeNull();
    }

    // ── OrderCompleted record should carry all IDs ───────────────────
    [Fact]
    public void OrderCompleted_ShouldCarryAllIds()
    {
        var corrId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var evt = new OrderCompleted
        {
            CorrelationId = corrId,
            OrderId = orderId,
            VehicleId = vehicleId,
            PaymentId = paymentId
        };

        evt.CorrelationId.Should().Be(corrId);
        evt.OrderId.Should().Be(orderId);
        evt.VehicleId.Should().Be(vehicleId);
        evt.PaymentId.Should().Be(paymentId);
    }
}
