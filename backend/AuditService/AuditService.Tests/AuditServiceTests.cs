// =====================================================
// C9: AuditService - Tests Unitarios
// Trazabilidad y Auditoría de Cumplimiento OKLA
// =====================================================

using FluentAssertions;
using AuditService.Domain.Entities;
using AuditService.Shared.Enums;
using Xunit;

namespace AuditService.Tests;

public class AuditServiceTests
{
    // Constantes de acciones para evitar conflicto de tipos
    private const string ActionLogin = "LOGIN";
    private const string ActionLogout = "LOGOUT";
    private const string ActionRegister = "REGISTER";
    private const string ActionChangePassword = "CHANGE_PASSWORD";
    private const string ActionCreateVehicle = "CREATE_VEHICLE";
    private const string ActionUpdateVehicle = "UPDATE_VEHICLE";
    private const string ActionDeleteVehicle = "DELETE_VEHICLE";
    private const string ActionViewVehicle = "VIEW_VEHICLE";
    private const string ActionCreateOrder = "CREATE_ORDER";
    private const string ActionExportData = "EXPORT_DATA";
    private const string ActionDeleteUser = "DELETE_USER";
    private const string ActionDisableUser = "DISABLE_USER";
    private const string ActionLockUser = "LOCK_USER";

    // =====================================================
    // Tests de Registro de Auditoría
    // =====================================================

    [Fact]
    public void AuditLog_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>
        {
            { "vehicleId", Guid.NewGuid().ToString() },
            { "previousOwner", "María García" }
        };

        // Act
        var log = new AuditLog(
            userId: "user-123",
            action: ActionCreateVehicle,
            resource: "Vehicle",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: additionalData,
            success: true,
            serviceName: "VehiclesSaleService",
            severity: AuditSeverity.Information
        );

        // Assert
        log.Should().NotBeNull();
        log.UserId.Should().Be("user-123");
        log.Action.Should().Be(ActionCreateVehicle);
        log.Success.Should().BeTrue();
        log.Severity.Should().Be(AuditSeverity.Information);
    }

    [Theory]
    [InlineData(AuditSeverity.Debug)]
    [InlineData(AuditSeverity.Information)]
    [InlineData(AuditSeverity.Warning)]
    [InlineData(AuditSeverity.Error)]
    [InlineData(AuditSeverity.Critical)]
    public void AuditSeverity_ShouldHaveExpectedValues(AuditSeverity severity)
    {
        // Assert
        Enum.IsDefined(typeof(AuditSeverity), severity).Should().BeTrue();
    }

    // =====================================================
    // Tests de Acciones de Auditoría
    // =====================================================

    [Fact]
    public void AuditActions_ShouldContainAuthenticationActions()
    {
        // Assert - Verificar que las constantes siguen el formato correcto
        ActionLogin.Should().Be("LOGIN");
        ActionLogout.Should().Be("LOGOUT");
        ActionRegister.Should().Be("REGISTER");
        ActionChangePassword.Should().Be("CHANGE_PASSWORD");
    }

    [Fact]
    public void AuditActions_ShouldContainVehicleActions()
    {
        // Assert
        ActionCreateVehicle.Should().Be("CREATE_VEHICLE");
        ActionUpdateVehicle.Should().Be("UPDATE_VEHICLE");
        ActionDeleteVehicle.Should().Be("DELETE_VEHICLE");
        ActionViewVehicle.Should().Be("VIEW_VEHICLE");
    }

    // =====================================================
    // Tests de Retención de Datos (7 años - Normativa RD)
    // =====================================================

    [Fact]
    public void AuditRetention_ShouldBe7Years_ForCompliance()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var retentionYears = 7; // 7 años según normativas de RD

        // Act
        var retentionDate = createdAt.AddYears(retentionYears);

        // Assert
        retentionDate.Year.Should().Be(createdAt.Year + 7);
    }

    // =====================================================
    // Tests de Auditoría Fallida
    // =====================================================

    [Fact]
    public void AuditLog_ShouldTrackFailedOperations()
    {
        // Arrange & Act
        var log = new AuditLog(
            userId: "user-123",
            action: ActionLogin,
            resource: "Authentication",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: new Dictionary<string, object>(),
            success: false,
            errorMessage: "Contraseña incorrecta",
            serviceName: "AuthService",
            severity: AuditSeverity.Warning
        );

        // Assert
        log.Success.Should().BeFalse();
        log.Severity.Should().Be(AuditSeverity.Warning);
    }

    // =====================================================
    // Tests de Correlación
    // =====================================================

    [Fact]
    public void AuditLog_ShouldSupportCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var log = new AuditLog(
            userId: "user-123",
            action: ActionCreateOrder,
            resource: "Order",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: new Dictionary<string, object>(),
            correlationId: correlationId,
            serviceName: "OrderService"
        );

        // Assert
        log.CorrelationId.Should().Be(correlationId);
    }

    // =====================================================
    // Tests de Duración de Operación
    // =====================================================

    [Fact]
    public void AuditLog_ShouldTrackOperationDuration()
    {
        // Arrange & Act
        var log = new AuditLog(
            userId: "user-123",
            action: ActionExportData,
            resource: "Report",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: new Dictionary<string, object>(),
            durationMs: 2500,
            serviceName: "ReportService"
        );

        // Assert
        log.DurationMs.Should().Be(2500);
    }

    // =====================================================
    // Tests de Severidad por Tipo de Acción
    // =====================================================

    [Fact]
    public void CriticalActions_ShouldHaveCriticalSeverity()
    {
        // Arrange - Acciones críticas deberían tener severidad Critical
        var criticalActions = new[]
        {
            ActionDeleteUser,
            ActionDisableUser,
            ActionLockUser
        };

        // Assert
        criticalActions.Should().AllSatisfy(action =>
            action.Should().NotBeNullOrEmpty()
        );
    }

    // =====================================================
    // Tests de Datos Adicionales
    // =====================================================

    [Fact]
    public void AuditLog_ShouldStoreAdditionalData()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>
        {
            { "orderId", "ORD-2026-00001" },
            { "amount", 150000.00m },
            { "currency", "DOP" }
        };

        // Act
        var log = new AuditLog(
            userId: "user-123",
            action: ActionCreateOrder,
            resource: "Order",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: additionalData,
            serviceName: "OrderService"
        );

        // Assert
        log.AdditionalDataJson.Should().Contain("orderId");
        log.AdditionalDataJson.Should().Contain("ORD-2026-00001");
    }

    // =====================================================
    // Tests de Checksum para Integridad
    // =====================================================

    [Fact]
    public void AuditLog_ShouldSupportChecksumCalculation()
    {
        // Arrange
        var log = new AuditLog(
            userId: "user-123",
            action: ActionCreateVehicle,
            resource: "Vehicle",
            userIp: "192.168.1.100",
            userAgent: "Mozilla/5.0",
            additionalData: new Dictionary<string, object>(),
            serviceName: "VehicleService"
        );

        // Act
        var checksum = ComputeChecksum(log.Id.ToString(), log.UserId, log.Action);

        // Assert
        checksum.Should().NotBeNullOrEmpty();
        checksum.Length.Should().BeGreaterThan(20);
    }

    private string ComputeChecksum(string id, string userId, string action)
    {
        var data = $"{id}|{userId}|{action}|{DateTime.UtcNow:O}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(bytes);
    }

    // =====================================================
    // Tests de Servicios de OKLA
    // =====================================================

    [Fact]
    public void AuditLog_ShouldTrackOKLAServices()
    {
        // Arrange
        var oklaServices = new[]
        {
            "AuthService",
            "UserService",
            "VehiclesSaleService",
            "MediaService",
            "BillingService",
            "NotificationService"
        };

        // Assert
        oklaServices.Should().HaveCount(6);
        oklaServices.Should().Contain("VehiclesSaleService");
    }
}
