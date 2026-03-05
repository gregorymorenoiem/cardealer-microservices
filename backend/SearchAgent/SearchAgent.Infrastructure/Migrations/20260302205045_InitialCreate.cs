using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_agent_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    MinResultsPerPage = table.Column<int>(type: "integer", nullable: false),
                    MaxResultsPerPage = table.Column<int>(type: "integer", nullable: false),
                    SponsoredAffinityThreshold = table.Column<float>(type: "real", nullable: false),
                    MaxSponsoredPercentage = table.Column<float>(type: "real", nullable: false),
                    SponsoredPositions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SponsoredLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PriceRelaxPercent = table.Column<int>(type: "integer", nullable: false),
                    YearRelaxRange = table.Column<int>(type: "integer", nullable: false),
                    MaxRelaxationLevel = table.Column<int>(type: "integer", nullable: false),
                    EnableCache = table.Column<bool>(type: "boolean", nullable: false),
                    CacheTtlSeconds = table.Column<int>(type: "integer", nullable: false),
                    SemanticCacheThreshold = table.Column<float>(type: "real", nullable: false),
                    MaxQueriesPerMinutePerIp = table.Column<int>(type: "integer", nullable: false),
                    AiSearchTrafficPercent = table.Column<int>(type: "integer", nullable: false),
                    SystemPromptOverride = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_agent_config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "search_queries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalQuery = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReformulatedQuery = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    FiltersJson = table.Column<string>(type: "jsonb", nullable: true),
                    Confidence = table.Column<float>(type: "real", nullable: false),
                    FilterLevel = table.Column<int>(type: "integer", nullable: false),
                    OrganicResultCount = table.Column<int>(type: "integer", nullable: false),
                    SponsoredResultCount = table.Column<int>(type: "integer", nullable: false),
                    TotalResultCount = table.Column<int>(type: "integer", nullable: false),
                    LatencyMs = table.Column<int>(type: "integer", nullable: false),
                    WasCached = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_queries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "search_agent_config",
                columns: new[] { "Id", "AiSearchTrafficPercent", "CacheTtlSeconds", "CreatedAt", "EnableCache", "IsEnabled", "MaxQueriesPerMinutePerIp", "MaxRelaxationLevel", "MaxResultsPerPage", "MaxSponsoredPercentage", "MaxTokens", "MinResultsPerPage", "Model", "PriceRelaxPercent", "SemanticCacheThreshold", "SponsoredAffinityThreshold", "SponsoredLabel", "SponsoredPositions", "SystemPromptOverride", "Temperature", "UpdatedAt", "UpdatedBy", "YearRelaxRange" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 100, 3600, new DateTime(2026, 3, 2, 20, 50, 45, 541, DateTimeKind.Utc).AddTicks(2440), true, true, 60, 5, 40, 0.25f, 1024, 8, "claude-haiku-4-5-20251001", 25, 0.95f, 0.45f, "Patrocinado", "1,5,10", null, 0.2f, new DateTime(2026, 3, 2, 20, 50, 45, 541, DateTimeKind.Utc).AddTicks(2440), null, 2 });

            migrationBuilder.CreateIndex(
                name: "IX_search_queries_CreatedAt",
                table: "search_queries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_search_queries_UserId",
                table: "search_queries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_agent_config");

            migrationBuilder.DropTable(
                name: "search_queries");
        }
    }
}
