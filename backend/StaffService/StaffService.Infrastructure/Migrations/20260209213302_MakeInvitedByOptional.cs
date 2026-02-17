using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeInvitedByOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staff_invitations_staff_InvitedBy",
                table: "staff_invitations");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitedBy",
                table: "staff_invitations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 21, 33, 2, 558, DateTimeKind.Utc).AddTicks(260));

            migrationBuilder.AddForeignKey(
                name: "FK_staff_invitations_staff_InvitedBy",
                table: "staff_invitations",
                column: "InvitedBy",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staff_invitations_staff_InvitedBy",
                table: "staff_invitations");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitedBy",
                table: "staff_invitations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.UpdateData(
                table: "positions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 17, 54, 58, 697, DateTimeKind.Utc).AddTicks(8400));

            migrationBuilder.AddForeignKey(
                name: "FK_staff_invitations_staff_InvitedBy",
                table: "staff_invitations",
                column: "InvitedBy",
                principalTable: "staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
