using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsForBusinessKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_waybills_waybill_number",
                table: "waybills",
                newName: "ix_waybills_waybill_number_trgm");

            migrationBuilder.RenameIndex(
                name: "IX_contracts_contract_number",
                table: "contracts",
                newName: "ix_contracts_contract_number_trgm");

            migrationBuilder.CreateIndex(
                name: "ix_waybills_waybill_number_unique",
                table: "waybills",
                column: "waybill_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_code",
                table: "products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contracts_contract_number_unique",
                table: "contracts",
                column: "contract_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_waybills_waybill_number_unique",
                table: "waybills");

            migrationBuilder.DropIndex(
                name: "IX_products_code",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_contracts_contract_number_unique",
                table: "contracts");

            migrationBuilder.RenameIndex(
                name: "ix_waybills_waybill_number_trgm",
                table: "waybills",
                newName: "IX_waybills_waybill_number");

            migrationBuilder.RenameIndex(
                name: "ix_contracts_contract_number_trgm",
                table: "contracts",
                newName: "IX_contracts_contract_number");
        }
    }
}
