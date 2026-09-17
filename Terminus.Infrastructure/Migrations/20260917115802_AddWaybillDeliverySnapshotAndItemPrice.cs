using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWaybillDeliverySnapshotAndItemPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "delivery_base_cost",
                table: "waybills",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "delivery_commission_cost",
                table: "waybills",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "delivery_transport_multiplier",
                table: "waybills",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "waybill_items",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivery_base_cost",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "delivery_commission_cost",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "delivery_transport_multiplier",
                table: "waybills");

            migrationBuilder.DropColumn(
                name: "price",
                table: "waybill_items");
        }
    }
}
