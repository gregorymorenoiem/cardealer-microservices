using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackupDRService.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialBackupDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AdditionalData = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "retention_policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DailyRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    WeeklyRetentionWeeks = table.Column<int>(type: "integer", nullable: false),
                    MonthlyRetentionMonths = table.Column<int>(type: "integer", nullable: false),
                    YearlyRetentionYears = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    MaxBackupCount = table.Column<int>(type: "integer", nullable: true),
                    DeleteOldestWhenLimitReached = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveOldBackups = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveStorageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ArchivePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ArchiveAfterDays = table.Column<int>(type: "integer", nullable: false),
                    RequireSuccessfulBackupBeforeDelete = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyBeforeDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retention_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "backup_schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DatabaseName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectionString = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BackupType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StorageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RetentionDays = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompressBackup = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptBackup = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptionKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RetentionPolicyId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backup_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_backup_schedules_retention_policies_RetentionPolicyId",
                        column: x => x.RetentionPolicyId,
                        principalTable: "retention_policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "backup_histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BackupId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DatabaseName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BackupType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StorageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsCompressed = table.Column<bool>(type: "boolean", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    Checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScheduleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backup_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_backup_histories_backup_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "backup_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action",
                table: "audit_logs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType",
                table: "audit_logs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Status",
                table: "audit_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Timestamp",
                table: "audit_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_BackupId",
                table: "backup_histories",
                column: "BackupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_DatabaseName",
                table: "backup_histories",
                column: "DatabaseName");

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_JobId",
                table: "backup_histories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_ScheduleId",
                table: "backup_histories",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_StartedAt",
                table: "backup_histories",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_backup_histories_Status",
                table: "backup_histories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_backup_schedules_DatabaseName",
                table: "backup_schedules",
                column: "DatabaseName");

            migrationBuilder.CreateIndex(
                name: "IX_backup_schedules_IsEnabled",
                table: "backup_schedules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_backup_schedules_Name",
                table: "backup_schedules",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_backup_schedules_NextRunAt",
                table: "backup_schedules",
                column: "NextRunAt");

            migrationBuilder.CreateIndex(
                name: "IX_backup_schedules_RetentionPolicyId",
                table: "backup_schedules",
                column: "RetentionPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_retention_policies_IsActive",
                table: "retention_policies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_retention_policies_Name",
                table: "retention_policies",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "backup_histories");

            migrationBuilder.DropTable(
                name: "backup_schedules");

            migrationBuilder.DropTable(
                name: "retention_policies");
        }
    }
}
