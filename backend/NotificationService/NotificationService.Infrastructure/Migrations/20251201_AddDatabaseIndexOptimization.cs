using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20251201_AddDatabaseIndexOptimization")]
public class AddDatabaseIndexOptimization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Índice compuesto para consultas frecuentes por UserId y fecha
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserId", "CreatedAt" });

        // Índice para búsquedas por canal y estado
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_Channel_Status",
            table: "Notifications",
            columns: new[] { "Channel", "Status" });

        // Índice para búsquedas por estado de entrega
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_Status_DeliveredAt",
            table: "Notifications",
            columns: new[] { "Status", "DeliveredAt" });

        // Índice para búsquedas por tipo de notificación
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_Type",
            table: "Notifications",
            column: "Type");

        // Índice para consultas de fecha de creación (reportes)
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_CreatedAt",
            table: "Notifications",
            column: "CreatedAt");

        // Índice para búsquedas por prioridad
        migrationBuilder.CreateIndex(
            name: "IX_Notifications_Priority_CreatedAt",
            table: "Notifications",
            columns: new[] { "Priority", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Notifications_UserId_CreatedAt", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notifications_Channel_Status", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notifications_Status_DeliveredAt", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notifications_Type", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notifications_CreatedAt", table: "Notifications");
        migrationBuilder.DropIndex(name: "IX_Notifications_Priority_CreatedAt", table: "Notifications");
    }
}
