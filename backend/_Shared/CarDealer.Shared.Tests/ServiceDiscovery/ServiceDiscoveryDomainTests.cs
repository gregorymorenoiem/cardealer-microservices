using ServiceDiscovery.Domain.Entities;
using ServiceDiscovery.Domain.Enums;
using FluentAssertions;

namespace CarDealer.Shared.Tests.ServiceDiscovery;

public class ServiceInstanceTests
{
    private static ServiceInstance CreateValid() => new()
    {
        Id = "auth-1",
        ServiceName = "auth-service",
        Host = "10.0.0.1",
        Port = 8080
    };

    // ── Default values ───────────────────────────────────────────────
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var si = new ServiceInstance();

        si.Id.Should().BeEmpty();
        si.ServiceName.Should().BeEmpty();
        si.Host.Should().BeEmpty();
        si.Port.Should().Be(0);
        si.Status.Should().Be(ServiceStatus.Active);
        si.HealthStatus.Should().Be(HealthStatus.Unknown);
        si.Tags.Should().BeEmpty();
        si.Metadata.Should().BeEmpty();
        si.HealthCheckUrl.Should().BeNull();
        si.HealthCheckInterval.Should().Be(10);
        si.HealthCheckTimeout.Should().Be(5);
        si.Version.Should().BeNull();
    }

    // ── Address computed property ────────────────────────────────────
    [Fact]
    public void Address_ShouldComputeFromHostAndPort()
    {
        var si = CreateValid();
        si.Address.Should().Be("http://10.0.0.1:8080");
    }

    // ── IsValid ──────────────────────────────────────────────────────
    [Fact]
    public void IsValid_ShouldReturnTrue_WhenAllFieldsSet()
    {
        CreateValid().IsValid().Should().BeTrue();
    }

    [Theory]
    [InlineData("", "svc", "host", 80)]
    [InlineData("id", "", "host", 80)]
    [InlineData("id", "svc", "", 80)]
    [InlineData("id", "svc", "host", 0)]
    [InlineData("id", "svc", "host", -1)]
    [InlineData("id", "svc", "host", 65536)]
    public void IsValid_ShouldReturnFalse_WhenFieldInvalid(
        string id, string serviceName, string host, int port)
    {
        var si = new ServiceInstance
        {
            Id = id, ServiceName = serviceName, Host = host, Port = port
        };
        si.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ShouldAcceptMaxPort()
    {
        var si = CreateValid();
        si.Port = 65535;
        si.IsValid().Should().BeTrue();
    }

    // ── IsHealthy ────────────────────────────────────────────────────
    [Fact]
    public void IsHealthy_ShouldReturnTrue_WhenActiveAndHealthy()
    {
        var si = CreateValid();
        si.Status = ServiceStatus.Active;
        si.HealthStatus = HealthStatus.Healthy;
        si.IsHealthy().Should().BeTrue();
    }

    [Theory]
    [InlineData(ServiceStatus.Inactive, HealthStatus.Healthy)]
    [InlineData(ServiceStatus.Deregistered, HealthStatus.Healthy)]
    [InlineData(ServiceStatus.Active, HealthStatus.Unhealthy)]
    [InlineData(ServiceStatus.Active, HealthStatus.Degraded)]
    [InlineData(ServiceStatus.Active, HealthStatus.Unknown)]
    public void IsHealthy_ShouldReturnFalse_WhenNotActiveOrNotHealthy(
        ServiceStatus status, HealthStatus healthStatus)
    {
        var si = CreateValid();
        si.Status = status;
        si.HealthStatus = healthStatus;
        si.IsHealthy().Should().BeFalse();
    }

    // ── UpdateHealth ─────────────────────────────────────────────────
    [Fact]
    public void UpdateHealth_ToHealthy_ShouldSetLastHealthyAt()
    {
        var si = CreateValid();
        si.LastHealthyAt.Should().BeNull();

        si.UpdateHealth(HealthStatus.Healthy);

        si.HealthStatus.Should().Be(HealthStatus.Healthy);
        si.LastCheckedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        si.LastHealthyAt.Should().NotBeNull();
        si.LastHealthyAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateHealth_ToUnhealthy_ShouldNotSetLastHealthyAt()
    {
        var si = CreateValid();
        si.UpdateHealth(HealthStatus.Unhealthy);

        si.HealthStatus.Should().Be(HealthStatus.Unhealthy);
        si.LastHealthyAt.Should().BeNull();
    }
}

public class HealthCheckResultTests
{
    // ── Factory: Healthy ─────────────────────────────────────────────
    [Fact]
    public void Healthy_ShouldReturnCorrectResult()
    {
        var result = HealthCheckResult.Healthy("auth-1", 42, 200);

        result.InstanceId.Should().Be("auth-1");
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ResponseTimeMs.Should().Be(42);
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be("Service is healthy");
        result.Error.Should().BeNull();
    }

    // ── Factory: Unhealthy ───────────────────────────────────────────
    [Fact]
    public void Unhealthy_ShouldReturnCorrectResult()
    {
        var result = HealthCheckResult.Unhealthy("auth-1", "Connection refused");

        result.InstanceId.Should().Be("auth-1");
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Error.Should().Be("Connection refused");
        result.Message.Should().Be("Service is unhealthy");
    }

    // ── Factory: Degraded ────────────────────────────────────────────
    [Fact]
    public void Degraded_ShouldReturnCorrectResult()
    {
        var result = HealthCheckResult.Degraded("auth-1", "High latency");

        result.InstanceId.Should().Be("auth-1");
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Message.Should().Be("High latency");
    }
}

public class ServiceCatalogTests
{
    private static ServiceInstance MakeInstance(
        string id, string svc, HealthStatus health = HealthStatus.Healthy) => new()
    {
        Id = id,
        ServiceName = svc,
        Host = "10.0.0.1",
        Port = 8080,
        Status = ServiceStatus.Active,
        HealthStatus = health
    };

    // ── RegisterService ──────────────────────────────────────────────
    [Fact]
    public void RegisterService_ShouldAddInstance()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth"));

        catalog.GetTotalInstanceCount().Should().Be(1);
        catalog.GetServiceNames().Should().Contain("auth");
    }

    [Fact]
    public void RegisterService_ShouldThrow_WhenInstanceInvalid()
    {
        var catalog = new ServiceCatalog();
        var invalid = new ServiceInstance(); // all defaults → IsValid() == false

        var act = () => catalog.RegisterService(invalid);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterService_ShouldReplaceExistingById()
    {
        var catalog = new ServiceCatalog();
        var inst1 = MakeInstance("a-1", "auth");
        inst1.Port = 8080;
        catalog.RegisterService(inst1);

        var inst2 = MakeInstance("a-1", "auth");
        inst2.Port = 9090;
        catalog.RegisterService(inst2);

        catalog.GetTotalInstanceCount().Should().Be(1);
        catalog.GetInstance("a-1")!.Port.Should().Be(9090);
    }

    // ── DeregisterService ────────────────────────────────────────────
    [Fact]
    public void DeregisterService_ShouldReturnTrue_WhenFound()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth"));

        catalog.DeregisterService("a-1").Should().BeTrue();
        catalog.GetTotalInstanceCount().Should().Be(0);
    }

    [Fact]
    public void DeregisterService_ShouldReturnFalse_WhenNotFound()
    {
        var catalog = new ServiceCatalog();
        catalog.DeregisterService("nonexistent").Should().BeFalse();
    }

    // ── GetServiceInstances ──────────────────────────────────────────
    [Fact]
    public void GetServiceInstances_ShouldReturnEmpty_WhenServiceUnknown()
    {
        var catalog = new ServiceCatalog();
        catalog.GetServiceInstances("unknown").Should().BeEmpty();
    }

    [Fact]
    public void GetServiceInstances_ShouldReturnCopy()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth"));

        var list = catalog.GetServiceInstances("auth");
        list.Add(MakeInstance("a-2", "auth")); // mutate copy

        catalog.GetServiceInstances("auth").Should().HaveCount(1); // original unchanged
    }

    // ── GetHealthyInstances ──────────────────────────────────────────
    [Fact]
    public void GetHealthyInstances_ShouldFilterCorrectly()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth", HealthStatus.Healthy));
        catalog.RegisterService(MakeInstance("a-2", "auth", HealthStatus.Unhealthy));
        catalog.RegisterService(MakeInstance("a-3", "auth", HealthStatus.Degraded));

        catalog.GetHealthyInstances("auth").Should().HaveCount(1);
        catalog.GetHealthyInstances("auth")[0].Id.Should().Be("a-1");
    }

    // ── GetInstance ──────────────────────────────────────────────────
    [Fact]
    public void GetInstance_ShouldReturnNull_WhenNotFound()
    {
        var catalog = new ServiceCatalog();
        catalog.GetInstance("nonexistent").Should().BeNull();
    }

    [Fact]
    public void GetInstance_ShouldFindAcrossServices()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth"));
        catalog.RegisterService(MakeInstance("m-1", "media"));

        catalog.GetInstance("m-1").Should().NotBeNull();
        catalog.GetInstance("m-1")!.ServiceName.Should().Be("media");
    }

    // ── Counts ───────────────────────────────────────────────────────
    [Fact]
    public void Counts_ShouldReflectRegisteredInstances()
    {
        var catalog = new ServiceCatalog();
        catalog.RegisterService(MakeInstance("a-1", "auth", HealthStatus.Healthy));
        catalog.RegisterService(MakeInstance("a-2", "auth", HealthStatus.Unhealthy));
        catalog.RegisterService(MakeInstance("m-1", "media", HealthStatus.Healthy));

        catalog.GetTotalInstanceCount().Should().Be(3);
        catalog.GetHealthyInstanceCount().Should().Be(2);
        catalog.GetServiceNames().Should().HaveCount(2);
    }
}

public class ServiceStatusEnumTests
{
    [Fact]
    public void ServiceStatus_ShouldHaveExpectedValues()
    {
        ServiceStatus.Active.Should().Be(ServiceStatus.Active);
        ((int)ServiceStatus.Active).Should().Be(0);
        ((int)ServiceStatus.Inactive).Should().Be(1);
        ((int)ServiceStatus.Deregistered).Should().Be(2);
    }
}

public class HealthStatusEnumTests
{
    [Fact]
    public void HealthStatus_ShouldHaveExpectedValues()
    {
        ((int)HealthStatus.Healthy).Should().Be(0);
        ((int)HealthStatus.Degraded).Should().Be(1);
        ((int)HealthStatus.Unhealthy).Should().Be(2);
        ((int)HealthStatus.Unknown).Should().Be(3);
    }
}
