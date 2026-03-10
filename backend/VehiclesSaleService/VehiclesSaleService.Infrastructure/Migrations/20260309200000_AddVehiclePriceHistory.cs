using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehiclePriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicle_price_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "DOP"),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_price_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_price_history_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_price_history_VehicleId",
                table: "vehicle_price_history",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_price_history_ChangedAt",
                table: "vehicle_price_history",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_price_history_VehicleId_ChangedAt",
                table: "vehicle_price_history",
                columns: new[] { "VehicleId", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_price_history");
        }
    }
}
