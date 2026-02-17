using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AIProcessingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "background_presets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    background_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    preview_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    floor_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shadow_intensity = table.Column<float>(type: "real", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    requires_dealer_membership = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_background_presets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "image_processing_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    original_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    processed_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    mask_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    max_retries = table.Column<int>(type: "integer", nullable: false),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processing_time_ms = table.Column<int>(type: "integer", nullable: false),
                    worker_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_processing_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spin360_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_video_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    frame_count = table.Column<int>(type: "integer", nullable: false),
                    source_image_urls = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    total_frames = table.Column<int>(type: "integer", nullable: false),
                    processed_frames = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processing_time_ms = table.Column<int>(type: "integer", nullable: false),
                    worker_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spin360_jobs", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "background_presets",
                columns: new[] { "id", "background_image_url", "code", "created_at", "description", "floor_color", "is_active", "is_default", "is_public", "name", "preview_url", "requires_dealer_membership", "shadow_intensity", "sort_order", "thumbnail_url", "type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "", "white_studio", new DateTime(2026, 1, 26, 13, 4, 59, 451, DateTimeKind.Utc).AddTicks(6000), "Fondo blanco profesional estilo catálogo", "#FFFFFF", true, true, true, "Blanco Infinito", "", false, 0.3f, 1, "", "Studio", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "", "gray_showroom", new DateTime(2026, 1, 26, 13, 4, 59, 451, DateTimeKind.Utc).AddTicks(6020), "Fondo de showroom profesional en gris", "#E5E5E5", true, false, true, "Showroom Gris", "", true, 0.4f, 2, "", "Showroom", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "", "dark_studio", new DateTime(2026, 1, 26, 13, 4, 59, 451, DateTimeKind.Utc).AddTicks(6020), "Fondo oscuro dramático para vehículos de lujo", "#1A1A1A", true, false, true, "Estudio Oscuro", "", true, 0.2f, 3, "", "Studio", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "", "outdoor_day", new DateTime(2026, 1, 26, 13, 4, 59, 451, DateTimeKind.Utc).AddTicks(6020), "Escena exterior con cielo azul", "#CCCCCC", true, false, true, "Exterior Día", "", true, 0.5f, 4, "", "Outdoor", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_background_presets_code",
                table: "background_presets",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_background_presets_is_active",
                table: "background_presets",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_image_processing_jobs_created_at",
                table: "image_processing_jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_image_processing_jobs_status",
                table: "image_processing_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_image_processing_jobs_user_id",
                table: "image_processing_jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_image_processing_jobs_vehicle_id",
                table: "image_processing_jobs",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_spin360_jobs_status",
                table: "spin360_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_spin360_jobs_user_id",
                table: "spin360_jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_spin360_jobs_vehicle_id",
                table: "spin360_jobs",
                column: "vehicle_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "background_presets");

            migrationBuilder.DropTable(
                name: "image_processing_jobs");

            migrationBuilder.DropTable(
                name: "spin360_jobs");
        }
    }
}
