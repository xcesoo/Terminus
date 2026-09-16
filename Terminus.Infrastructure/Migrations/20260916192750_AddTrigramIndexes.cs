using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terminus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigramIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_waybills_waybill_number",
                table: "waybills",
                column: "waybill_number")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_contracts_contract_number",
                table: "contracts",
                column: "contract_number")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_waybills_waybill_number",
                table: "waybills");

            migrationBuilder.DropIndex(
                name: "IX_contracts_contract_number",
                table: "contracts");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
