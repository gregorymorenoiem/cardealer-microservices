using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KYCService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    response_status_code = table.Column<int>(type: "integer", nullable: false),
                    response_body = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_processing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kyc_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kyc_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    risk_level = table.Column<int>(type: "integer", nullable: false),
                    risk_score = table.Column<int>(type: "integer", nullable: false),
                    risk_factors = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    place_of_birth = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    primary_document_type = table.Column<int>(type: "integer", nullable: false),
                    primary_document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    primary_document_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    primary_document_country = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    mobile_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    occupation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    source_of_funds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expected_transaction_volume = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estimated_annual_income = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_pep = table.Column<bool>(type: "boolean", nullable: false),
                    pep_position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pep_relationship = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    business_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    rnc = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    business_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    incorporation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    legal_representative = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    identity_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    address_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    income_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pep_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sanctions_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approval_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_review_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_review_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kyc_saga_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    current_step = table.Column<int>(type: "integer", nullable: false),
                    total_steps = table.Column<int>(type: "integer", nullable: false),
                    completed_steps_data = table.Column<string>(type: "text", nullable: false),
                    created_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_document_ids = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    failed_at_step = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rolled_back_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_saga_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rate_limit_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    request_count = table.Column<int>(type: "integer", nullable: false),
                    window_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    window_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_limit_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suspicious_transaction_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kyc_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    suspicious_activity_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    red_flags = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reporting_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uaf_report_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sent_to_uaf_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suspicious_transaction_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watchlist_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    list_type = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    aliases = table.Column<string>(type: "text", nullable: false),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    listed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watchlist_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kyc_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kyc_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    document_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    side = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    extracted_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    extracted_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    extracted_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_kyc_documents_kyc_profiles_kyc_profile_id",
                        column: x => x.kyc_profile_id,
                        principalTable: "kyc_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kyc_risk_assessments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kyc_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_level = table.Column<int>(type: "integer", nullable: false),
                    new_level = table.Column<int>(type: "integer", nullable: false),
                    previous_score = table.Column<int>(type: "integer", nullable: false),
                    new_score = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    factors = table.Column<string>(type: "text", nullable: false),
                    recommended_actions = table.Column<string>(type: "text", nullable: false),
                    assessed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    assessed_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    assessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_risk_assessments", x => x.id);
                    table.ForeignKey(
                        name: "FK_kyc_risk_assessments_kyc_profiles_kyc_profile_id",
                        column: x => x.kyc_profile_id,
                        principalTable: "kyc_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kyc_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kyc_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    raw_response = table.Column<string>(type: "text", nullable: true),
                    confidence_score = table.Column<int>(type: "integer", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_verifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_kyc_verifications_kyc_profiles_kyc_profile_id",
                        column: x => x.kyc_profile_id,
                        principalTable: "kyc_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_expires_at",
                table: "idempotency_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_key_user_id",
                table: "idempotency_keys",
                columns: new[] { "key", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kyc_audit_logs_action",
                table: "kyc_audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_audit_logs_created_at",
                table: "kyc_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_audit_logs_profile_id",
                table: "kyc_audit_logs",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_audit_logs_user_id",
                table: "kyc_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_documents_kyc_profile_id",
                table: "kyc_documents",
                column: "kyc_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_documents_status",
                table: "kyc_documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_is_pep",
                table: "kyc_profiles",
                column: "is_pep");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_primary_document_number",
                table: "kyc_profiles",
                column: "primary_document_number");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_risk_level",
                table: "kyc_profiles",
                column: "risk_level");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_rnc",
                table: "kyc_profiles",
                column: "rnc");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_status",
                table: "kyc_profiles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_profiles_user_id",
                table: "kyc_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kyc_risk_assessments_kyc_profile_id",
                table: "kyc_risk_assessments",
                column: "kyc_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_saga_states_correlation_id",
                table: "kyc_saga_states",
                column: "correlation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kyc_saga_states_status",
                table: "kyc_saga_states",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_saga_states_user_id",
                table: "kyc_saga_states",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_verifications_kyc_profile_id",
                table: "kyc_verifications",
                column: "kyc_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_kyc_verifications_verification_type",
                table: "kyc_verifications",
                column: "verification_type");

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_entries_key_endpoint",
                table: "rate_limit_entries",
                columns: new[] { "key", "endpoint" });

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_entries_window_end",
                table: "rate_limit_entries",
                column: "window_end");

            migrationBuilder.CreateIndex(
                name: "IX_suspicious_transaction_reports_report_number",
                table: "suspicious_transaction_reports",
                column: "report_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suspicious_transaction_reports_reporting_deadline",
                table: "suspicious_transaction_reports",
                column: "reporting_deadline");

            migrationBuilder.CreateIndex(
                name: "IX_suspicious_transaction_reports_status",
                table: "suspicious_transaction_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_suspicious_transaction_reports_user_id",
                table: "suspicious_transaction_reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_watchlist_entries_document_number",
                table: "watchlist_entries",
                column: "document_number");

            migrationBuilder.CreateIndex(
                name: "IX_watchlist_entries_full_name",
                table: "watchlist_entries",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "IX_watchlist_entries_is_active",
                table: "watchlist_entries",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_watchlist_entries_list_type",
                table: "watchlist_entries",
                column: "list_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_keys");

            migrationBuilder.DropTable(
                name: "kyc_audit_logs");

            migrationBuilder.DropTable(
                name: "kyc_documents");

            migrationBuilder.DropTable(
                name: "kyc_risk_assessments");

            migrationBuilder.DropTable(
                name: "kyc_saga_states");

            migrationBuilder.DropTable(
                name: "kyc_verifications");

            migrationBuilder.DropTable(
                name: "rate_limit_entries");

            migrationBuilder.DropTable(
                name: "suspicious_transaction_reports");

            migrationBuilder.DropTable(
                name: "watchlist_entries");

            migrationBuilder.DropTable(
                name: "kyc_profiles");
        }
    }
}
