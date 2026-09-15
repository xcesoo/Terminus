using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusWaybillsAndContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "is_terminated",
                table: "contracts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "dispatch_date",
                table: "waybills",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "waybills",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "conclusion_date",
                table: "contracts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "contracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "status",
                table: "contracts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "dispatch_date",
                table: "waybills",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "waybills",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "conclusion_date",
                table: "contracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_terminated",
                table: "contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
