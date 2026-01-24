using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerOnboardingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutoReplyMessage",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessAddress",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPhone",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredContactMethod",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RNC",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DealerOnboardingProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StepsCompleted = table.Column<string>(type: "text", nullable: false),
                    StepsSkipped = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerOnboardingProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealerOnboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    BusinessLegalName = table.Column<string>(type: "text", nullable: false),
                    RNC = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    MobilePhone = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Province = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    LegalRepName = table.Column<string>(type: "text", nullable: false),
                    LegalRepCedula = table.Column<string>(type: "text", nullable: false),
                    LegalRepPosition = table.Column<string>(type: "text", nullable: false),
                    AzulCustomerId = table.Column<string>(type: "text", nullable: true),
                    AzulSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    AzulCardToken = table.Column<string>(type: "text", nullable: true),
                    RequestedPlan = table.Column<int>(type: "integer", nullable: false),
                    IsEarlyBirdEligible = table.Column<bool>(type: "boolean", nullable: false),
                    IsEarlyBirdEnrolled = table.Column<bool>(type: "boolean", nullable: false),
                    RncDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessLicenseDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalRepCedulaDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SocialContractDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalPowerDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddressProofDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentsSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnderReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentSetupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerOnboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Features = table.Column<List<string>>(type: "text[]", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_onboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WasSkipped = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SkippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StepProfileCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StepProfileCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StepPreferencesCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StepPreferencesCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StepFirstSearchCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StepFirstSearchCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StepTourCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StepTourCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_onboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealerModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealerModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealerModules_ModuleId",
                table: "DealerModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "idx_user_onboardings_completed",
                table: "user_onboardings",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "idx_user_onboardings_skipped",
                table: "user_onboardings",
                column: "WasSkipped");

            migrationBuilder.CreateIndex(
                name: "idx_user_onboardings_user",
                table: "user_onboardings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealerModules");

            migrationBuilder.DropTable(
                name: "DealerOnboardingProcesses");

            migrationBuilder.DropTable(
                name: "DealerOnboardings");

            migrationBuilder.DropTable(
                name: "user_onboardings");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropColumn(
                name: "AutoReplyMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredContactMethod",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RNC",
                table: "Users");
        }
    }
}
