using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditService.Infrastructure.Migrations
{
    /// <summary>
    /// Migración para agregar índices de optimización de queries en AuditLog
    /// Mejora rendimiento de queries frecuentes: 
    /// - Búsquedas por UserId, Action, Resource, ServiceName
    /// - Filtros por Success, Severity, CreatedAt
    /// - Queries de compliance y seguridad
    /// </summary>
    public partial class AddDatabaseIndexOptimization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Índice compuesto para queries por usuario y fecha
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            // 2. Índice compuesto para queries por acción y éxito
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_Success_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Action", "Success", "CreatedAt" });

            // 3. Índice para queries por recurso
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Resource_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Resource", "CreatedAt" });

            // 4. Índice compuesto para queries de seguridad
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity_Success_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Severity", "Success", "CreatedAt" });

            // 5. Índice para queries por servicio
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ServiceName_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "ServiceName", "CreatedAt" });

            // 6. Índice para queries por CorrelationId (rastreo distribuido)
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs",
                column: "CorrelationId");

            // 7. Índice para queries por IP (análisis de seguridad)
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserIp_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserIp", "CreatedAt" });

            // 8. Índice solo por fecha (para limpieza y reporting)
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices en orden inverso
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserIp_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ServiceName_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Severity_Success_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Resource_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action_Success_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs");
        }
    }
}
