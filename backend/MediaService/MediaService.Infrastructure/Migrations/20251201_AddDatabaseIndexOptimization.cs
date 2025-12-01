using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MediaService.Infrastructure.Persistence;

namespace MediaService.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20251201_AddDatabaseIndexOptimization")]
public class AddDatabaseIndexOptimization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Índice compuesto para consultas frecuentes por UserId y fecha
        migrationBuilder.CreateIndex(
            name: "IX_Media_UserId_UploadedAt",
            table: "Media",
            columns: new[] { "UserId", "UploadedAt" });

        // Índice para búsquedas por tipo de media y estado
        migrationBuilder.CreateIndex(
            name: "IX_Media_MediaType_Status",
            table: "Media",
            columns: new[] { "MediaType", "Status" });

        // Índice para búsquedas por EntityId (VehicleId, etc.)
        migrationBuilder.CreateIndex(
            name: "IX_Media_EntityId_EntityType",
            table: "Media",
            columns: new[] { "EntityId", "EntityType" });

        // Índice para consultas de almacenamiento y limpieza
        migrationBuilder.CreateIndex(
            name: "IX_Media_Status_UploadedAt",
            table: "Media",
            columns: new[] { "Status", "UploadedAt" });

        // Índice para búsquedas por extensión de archivo (análisis de tipo)
        migrationBuilder.CreateIndex(
            name: "IX_Media_FileExtension",
            table: "Media",
            column: "FileExtension");

        // Índice para búsquedas por tamaño de archivo (análisis de almacenamiento)
        migrationBuilder.CreateIndex(
            name: "IX_Media_FileSizeBytes",
            table: "Media",
            column: "FileSizeBytes");

        // Índice para búsquedas por URL de almacenamiento
        migrationBuilder.CreateIndex(
            name: "IX_Media_StorageUrl",
            table: "Media",
            column: "StorageUrl",
            unique: true);

        // Índice para consultas de fecha de subida (reportes)
        migrationBuilder.CreateIndex(
            name: "IX_Media_UploadedAt",
            table: "Media",
            column: "UploadedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Media_UserId_UploadedAt", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_MediaType_Status", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_EntityId_EntityType", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_Status_UploadedAt", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_FileExtension", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_FileSizeBytes", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_StorageUrl", table: "Media");
        migrationBuilder.DropIndex(name: "IX_Media_UploadedAt", table: "Media");
    }
}
