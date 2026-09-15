using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelWaybillsAndTerminateContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "waybills",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_terminated",
                table: "contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "is_terminated",
                table: "contracts");
        }
    }
}
