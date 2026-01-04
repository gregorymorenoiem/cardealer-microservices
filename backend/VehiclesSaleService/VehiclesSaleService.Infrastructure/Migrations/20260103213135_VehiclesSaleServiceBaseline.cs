using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehiclesSaleService.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Baseline migration - Empty because database was already set up.
    /// This migration exists only to sync the EF Core model snapshot with the current database state.
    /// </summary>
    public partial class VehiclesSaleServiceBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: Database already has all tables and columns
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: Cannot rollback baseline
        }
    }
}
