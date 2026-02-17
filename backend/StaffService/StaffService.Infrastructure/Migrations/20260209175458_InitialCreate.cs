using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StaffService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultRole = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_positions_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_staff_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staff_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssignedRole = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmailSentCount = table.Column<int>(type: "integer", nullable: false),
                    LastEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_invitations_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_invitations_positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_invitations_staff_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_invitations_staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_invitations_staff_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staff_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GrantedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_permissions_staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "HeadId", "IsActive", "Name", "ParentDepartmentId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), "TECH", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", "Engineering and Development", null, true, "Technology", null, null, null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "OPS", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", "Business Operations", null, true, "Operations", null, null, null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "SUP", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", "Customer Service and Support", null, true, "Customer Support", null, null, null },
                    { new Guid("00000000-0000-0000-0001-000000000004"), "COMP", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", "Legal and Regulatory Compliance", null, true, "Compliance", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "positions",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DefaultRole", "DepartmentId", "Description", "IsActive", "Level", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), "SWE", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", 4, new Guid("00000000-0000-0000-0001-000000000001"), "Develops and maintains software applications", true, 2, "Software Engineer", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000002"), "SS", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", 4, new Guid("00000000-0000-0000-0001-000000000003"), "Provides customer support", true, 1, "Support Specialist", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000003"), "MOD", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", 3, new Guid("00000000-0000-0000-0001-000000000002"), "Moderates platform content", true, 1, "Content Moderator", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000004"), "CO", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", 2, new Guid("00000000-0000-0000-0001-000000000004"), "Ensures regulatory compliance", true, 3, "Compliance Officer", null, null },
                    { new Guid("00000000-0000-0000-0002-000000000005"), "SYSADMIN", new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400), "system", 1, new Guid("00000000-0000-0000-0001-000000000001"), "Manages system access and security", true, 3, "System Administrator", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_departments_Code",
                table: "departments",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_departments_HeadId",
                table: "departments",
                column: "HeadId");

            migrationBuilder.CreateIndex(
                name: "IX_departments_Name",
                table: "departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_ParentDepartmentId",
                table: "departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_positions_Code",
                table: "positions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_positions_DepartmentId",
                table: "positions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_positions_Title",
                table: "positions",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_staff_AuthUserId",
                table: "staff",
                column: "AuthUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_DepartmentId",
                table: "staff",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_Email",
                table: "staff",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_EmployeeCode",
                table: "staff",
                column: "EmployeeCode");

            migrationBuilder.CreateIndex(
                name: "IX_staff_PositionId",
                table: "staff",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_Role",
                table: "staff",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_staff_Status",
                table: "staff",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_staff_SupervisorId",
                table: "staff",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_DepartmentId",
                table: "staff_invitations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_Email",
                table: "staff_invitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_ExpiresAt",
                table: "staff_invitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_InvitedBy",
                table: "staff_invitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_PositionId",
                table: "staff_invitations",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_StaffId",
                table: "staff_invitations",
                column: "StaffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_Status",
                table: "staff_invitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_SupervisorId",
                table: "staff_invitations",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_invitations_Token",
                table: "staff_invitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_permissions_StaffId_Permission",
                table: "staff_permissions",
                columns: new[] { "StaffId", "Permission" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_departments_staff_HeadId",
                table: "departments",
                column: "HeadId",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_staff_HeadId",
                table: "departments");

            migrationBuilder.DropTable(
                name: "staff_invitations");

            migrationBuilder.DropTable(
                name: "staff_permissions");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
