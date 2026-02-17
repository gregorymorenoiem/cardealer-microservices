using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reviewservice");

            migrationBuilder.CreateTable(
                name: "review_summaries",
                schema: "reviewservice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    FiveStarReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FourStarReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ThreeStarReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TwoStarReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OneStarReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PositivePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    VerifiedPurchaseReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_summaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "reviewservice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModeratedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BuyerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuyerPhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HelpfulVotes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalVotes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TrustScore = table.Column<int>(type: "integer", nullable: false),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    FlagReason = table.Column<string>(type: "text", nullable: true),
                    WasAutoRequested = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserIpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "seller_badges",
                schema: "reviewservice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    badge_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Criteria = table.Column<string>(type: "text", nullable: true),
                    qualifying_stats = table.Column<string>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller_badges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fraud_detection_logs",
                schema: "reviewservice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confidence_score = table.Column<int>(type: "integer", nullable: false),
                    details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AlgorithmVersion = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fraud_detection_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_fraud_detection_logs_reviews_review_id",
                        column: x => x.review_id,
                        principalSchema: "reviewservice",
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_helpful_votes",
                schema: "reviewservice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_helpful = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    voted_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    user_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_helpful_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_helpful_votes_reviews_review_id",
                        column: x => x.review_id,
                        principalSchema: "reviewservice",
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_requests",
                schema: "reviewservice",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    buyer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    request_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reminders_sent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_reminder_sent = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_requests_reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalSchema: "reviewservice",
                        principalTable: "reviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "review_responses",
                schema: "reviewservice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SellerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_responses_reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalSchema: "reviewservice",
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_check_type",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                column: "check_type");

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_checked_at",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                column: "checked_at");

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_confidence_score",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                column: "confidence_score");

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_result",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                column: "result");

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_review_id",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_fraud_detection_logs_type_result",
                schema: "reviewservice",
                table: "fraud_detection_logs",
                columns: new[] { "check_type", "result" });

            migrationBuilder.CreateIndex(
                name: "ix_review_helpful_votes_ip_address",
                schema: "reviewservice",
                table: "review_helpful_votes",
                column: "user_ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_review_helpful_votes_review_id",
                schema: "reviewservice",
                table: "review_helpful_votes",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_helpful_votes_review_user",
                schema: "reviewservice",
                table: "review_helpful_votes",
                columns: new[] { "review_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_review_helpful_votes_user_id",
                schema: "reviewservice",
                table: "review_helpful_votes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_helpful_votes_voted_at",
                schema: "reviewservice",
                table: "review_helpful_votes",
                column: "voted_at");

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_buyer_email",
                schema: "reviewservice",
                table: "review_requests",
                column: "buyer_email");

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_expires_at",
                schema: "reviewservice",
                table: "review_requests",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_requested_at",
                schema: "reviewservice",
                table: "review_requests",
                column: "requested_at");

            migrationBuilder.CreateIndex(
                name: "IX_review_requests_ReviewId",
                schema: "reviewservice",
                table: "review_requests",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_seller_buyer_vehicle",
                schema: "reviewservice",
                table: "review_requests",
                columns: new[] { "seller_id", "buyer_id", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_status",
                schema: "reviewservice",
                table: "review_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_status_expires",
                schema: "reviewservice",
                table: "review_requests",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_review_requests_token",
                schema: "reviewservice",
                table: "review_requests",
                column: "request_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_responses_review_id",
                schema: "reviewservice",
                table: "review_responses",
                column: "ReviewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_responses_seller_id",
                schema: "reviewservice",
                table: "review_responses",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_review_summaries_average_rating",
                schema: "reviewservice",
                table: "review_summaries",
                column: "AverageRating");

            migrationBuilder.CreateIndex(
                name: "IX_review_summaries_seller_id",
                schema: "reviewservice",
                table: "review_summaries",
                column: "SellerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_summaries_total_reviews",
                schema: "reviewservice",
                table: "review_summaries",
                column: "TotalReviews");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_approved_rating",
                schema: "reviewservice",
                table: "reviews",
                columns: new[] { "IsApproved", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_buyer_id",
                schema: "reviewservice",
                table: "reviews",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_order_id",
                schema: "reviewservice",
                table: "reviews",
                column: "OrderId",
                unique: true,
                filter: "\"OrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_seller_id",
                schema: "reviewservice",
                table: "reviews",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_unique_buyer_seller_vehicle",
                schema: "reviewservice",
                table: "reviews",
                columns: new[] { "BuyerId", "SellerId", "VehicleId" },
                unique: true,
                filter: "\"VehicleId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_seller_badges_badge_type",
                schema: "reviewservice",
                table: "seller_badges",
                column: "badge_type");

            migrationBuilder.CreateIndex(
                name: "ix_seller_badges_granted_at",
                schema: "reviewservice",
                table: "seller_badges",
                column: "granted_at");

            migrationBuilder.CreateIndex(
                name: "ix_seller_badges_is_active",
                schema: "reviewservice",
                table: "seller_badges",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_seller_badges_seller_id",
                schema: "reviewservice",
                table: "seller_badges",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "ix_seller_badges_seller_type_active",
                schema: "reviewservice",
                table: "seller_badges",
                columns: new[] { "seller_id", "badge_type", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fraud_detection_logs",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "review_helpful_votes",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "review_requests",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "review_responses",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "review_summaries",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "seller_badges",
                schema: "reviewservice");

            migrationBuilder.DropTable(
                name: "reviews",
                schema: "reviewservice");
        }
    }
}
