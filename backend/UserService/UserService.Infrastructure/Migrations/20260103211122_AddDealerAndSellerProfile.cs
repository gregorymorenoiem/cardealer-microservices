using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerAndSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: These columns already exist in the database - skipping
            // migrationBuilder.AddColumn<int>(name: "AccountType"...
            // migrationBuilder.AddColumn<Guid>(name: "CreatedBy"...
            // migrationBuilder.AddColumn<Guid>(name: "DealerId"...
            // migrationBuilder.AddColumn<string>(name: "DealerPermissions"...
            // migrationBuilder.AddColumn<int>(name: "DealerRole"...
            // migrationBuilder.AddColumn<Guid>(name: "EmployerUserId"...
            // migrationBuilder.AddColumn<string>(name: "PlatformPermissions"...
            // migrationBuilder.AddColumn<int>(name: "PlatformRole"...

            migrationBuilder.CreateTable(
                name: "DealerEmployeeInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DealerRole = table.Column<int>(type: "integer", nullable: false),
                    Permissions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: "[]"),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvitationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerEmployeeInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealerEmployeeInvitations_Users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DealerType = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    WhatsApp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "DO"),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BannerUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BusinessRegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DealerLicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessLicenseDocumentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalListings = table.Column<int>(type: "integer", nullable: false),
                    ActiveListings = table.Column<int>(type: "integer", nullable: false),
                    TotalSales = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    ResponseTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptsFinancing = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptsTradeIn = table.Column<bool>(type: "boolean", nullable: false),
                    OffersWarranty = table.Column<bool>(type: "boolean", nullable: false),
                    HomeDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    BusinessHours = table.Column<string>(type: "jsonb", nullable: true),
                    SocialMediaLinks = table.Column<string>(type: "jsonb", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaxListings = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    FeaturedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Features = table.Column<string>(type: "text", nullable: false),
                    CurrentListings = table.Column<int>(type: "integer", nullable: false),
                    ListingsThisMonth = table.Column<int>(type: "integer", nullable: false),
                    FeaturedUsed = table.Column<int>(type: "integer", nullable: false),
                    StripeSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LongDescription = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HasFreeTrial = table.Column<bool>(type: "boolean", nullable: false),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    RequiredModules = table.Column<string>(type: "text", nullable: false),
                    IncludedInPlans = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    DocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleAddons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformEmployeeInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PlatformRole = table.Column<int>(type: "integer", nullable: false),
                    Permissions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: "[]"),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvitationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformEmployeeInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformEmployeeInvitations_Users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformRole = table.Column<int>(type: "integer", nullable: false),
                    Permissions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: "[]"),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformEmployees_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformEmployees_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformEmployees_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AlternatePhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsApp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "DO"),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerificationExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalListings = table.Column<int>(type: "integer", nullable: false),
                    ActiveListings = table.Column<int>(type: "integer", nullable: false),
                    TotalSales = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false),
                    ResponseTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptsOffers = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPhone = table.Column<bool>(type: "boolean", nullable: false),
                    ShowLocation = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredContactMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaxActiveListings = table.Column<int>(type: "integer", nullable: false),
                    CanSellHighValue = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealerEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerRole = table.Column<int>(type: "integer", nullable: false),
                    Permissions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: "[]"),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvitationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ActivationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealerEmployees_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DealerEmployees_Users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealerEmployees_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealerEmployees_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromPlan = table.Column<int>(type: "integer", nullable: false),
                    ToPlan = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionHistory_DealerSubscriptions_DealerSubscriptionId",
                        column: x => x.DealerSubscriptionId,
                        principalTable: "DealerSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealerModuleSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleAddonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsYearlyBilling = table.Column<bool>(type: "boolean", nullable: false),
                    StripeSubscriptionItemId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerModuleSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealerModuleSubscriptions_ModuleAddons_ModuleAddonId",
                        column: x => x.ModuleAddonId,
                        principalTable: "ModuleAddons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DealerModuleSubscriptions_Users_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuingCountry = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FrontImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BackImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SelfieWithDocumentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExtractedFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExtractedDateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtractedAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastViewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityDocuments_SellerProfiles_SellerProfileId",
                        column: x => x.SellerProfileId,
                        principalTable: "SellerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployeeInvitations_Email",
                table: "DealerEmployeeInvitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployeeInvitations_InvitedBy",
                table: "DealerEmployeeInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployeeInvitations_Status",
                table: "DealerEmployeeInvitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployeeInvitations_Token",
                table: "DealerEmployeeInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployees_DealerId",
                table: "DealerEmployees",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployees_InvitedBy",
                table: "DealerEmployees",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployees_Status",
                table: "DealerEmployees",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployees_UserId",
                table: "DealerEmployees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DealerEmployees_UserId1",
                table: "DealerEmployees",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_DealerModuleSubscriptions_DealerId",
                table: "DealerModuleSubscriptions",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_DealerModuleSubscriptions_ModuleAddonId",
                table: "DealerModuleSubscriptions",
                column: "ModuleAddonId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_BusinessRegistrationNumber",
                table: "Dealers",
                column: "BusinessRegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_City",
                table: "Dealers",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_Email",
                table: "Dealers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_Latitude_Longitude",
                table: "Dealers",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_OwnerUserId",
                table: "Dealers",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_VerificationStatus",
                table: "Dealers",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityDocuments_DocumentType_DocumentNumber",
                table: "IdentityDocuments",
                columns: new[] { "DocumentType", "DocumentNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityDocuments_SellerProfileId",
                table: "IdentityDocuments",
                column: "SellerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityDocuments_Status",
                table: "IdentityDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployeeInvitations_Email",
                table: "PlatformEmployeeInvitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployeeInvitations_InvitedBy",
                table: "PlatformEmployeeInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployeeInvitations_Status",
                table: "PlatformEmployeeInvitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployeeInvitations_Token",
                table: "PlatformEmployeeInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployees_AssignedBy",
                table: "PlatformEmployees",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployees_Status",
                table: "PlatformEmployees",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployees_UserId",
                table: "PlatformEmployees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEmployees_UserId1",
                table: "PlatformEmployees",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_City",
                table: "SellerProfiles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_Latitude_Longitude",
                table: "SellerProfiles",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_UserId",
                table: "SellerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_VerificationStatus",
                table: "SellerProfiles",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistory_DealerSubscriptionId",
                table: "SubscriptionHistory",
                column: "DealerSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealerEmployeeInvitations");

            migrationBuilder.DropTable(
                name: "DealerEmployees");

            migrationBuilder.DropTable(
                name: "DealerModuleSubscriptions");

            migrationBuilder.DropTable(
                name: "IdentityDocuments");

            migrationBuilder.DropTable(
                name: "PlatformEmployeeInvitations");

            migrationBuilder.DropTable(
                name: "PlatformEmployees");

            migrationBuilder.DropTable(
                name: "SubscriptionHistory");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "ModuleAddons");

            migrationBuilder.DropTable(
                name: "SellerProfiles");

            migrationBuilder.DropTable(
                name: "DealerSubscriptions");

            // NOTE: These columns existed before this migration - do NOT drop
            // migrationBuilder.DropColumn(name: "AccountType", table: "Users");
            // migrationBuilder.DropColumn(name: "CreatedBy", table: "Users");
            // migrationBuilder.DropColumn(name: "DealerId", table: "Users");
            // migrationBuilder.DropColumn(name: "DealerPermissions", table: "Users");
            // migrationBuilder.DropColumn(name: "DealerRole", table: "Users");
            // migrationBuilder.DropColumn(name: "EmployerUserId", table: "Users");
            // migrationBuilder.DropColumn(name: "PlatformPermissions", table: "Users");
            // migrationBuilder.DropColumn(name: "PlatformRole", table: "Users");
        }
    }
}
