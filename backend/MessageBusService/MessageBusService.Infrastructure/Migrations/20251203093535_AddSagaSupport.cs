using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessageBusService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sagas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Context = table.Column<string>(type: "jsonb", nullable: true),
                    CurrentStepIndex = table.Column<int>(type: "integer", nullable: false),
                    TotalSteps = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    CurrentRetryAttempt = table.Column<int>(type: "integer", nullable: false),
                    Timeout = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sagas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SagaSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SagaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionPayload = table.Column<string>(type: "text", nullable: false),
                    CompensationActionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompensationPayload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompensationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompensationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ResponsePayload = table.Column<string>(type: "text", nullable: true),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    Timeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SagaSteps_Sagas_SagaId",
                        column: x => x.SagaId,
                        principalTable: "Sagas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sagas_CorrelationId",
                table: "Sagas",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sagas_CreatedAt",
                table: "Sagas",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sagas_Status",
                table: "Sagas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SagaSteps_SagaId",
                table: "SagaSteps",
                column: "SagaId");

            migrationBuilder.CreateIndex(
                name: "IX_SagaSteps_SagaId_Order",
                table: "SagaSteps",
                columns: new[] { "SagaId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_SagaSteps_Status",
                table: "SagaSteps",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaSteps");

            migrationBuilder.DropTable(
                name: "Sagas");
        }
    }
}
