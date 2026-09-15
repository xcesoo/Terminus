using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consumers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    bank_account = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    price_list_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    conclusion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    consumer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracts_consumers_consumer_id",
                        column: x => x.consumer_id,
                        principalTable: "consumers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "waybills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    waybill_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dispatch_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    shipped_quantity = table.Column<int>(type: "integer", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    car_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    route_sheet_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    auto_service_sum = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    flight_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    avia_receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    avia_service_sum = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    container_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    railway_receipt_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    train_service_sum = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waybills", x => x.id);
                    table.ForeignKey(
                        name: "FK_waybills_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_consumer_id",
                table: "contracts",
                column: "consumer_id");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_product_id",
                table: "contracts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_waybills_contract_id",
                table: "waybills",
                column: "contract_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "waybills");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "consumers");

            migrationBuilder.DropTable(
                name: "products");
        }
    }
}
