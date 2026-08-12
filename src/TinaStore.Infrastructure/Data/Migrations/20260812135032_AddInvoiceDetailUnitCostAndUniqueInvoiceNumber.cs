using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinaStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDetailUnitCostAndUniqueInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "InvoiceDetails",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            // Conserva el número de la primera factura de cada grupo y renumera
            // únicamente las repetidas antes de imponer la restricción única.
            migrationBuilder.Sql("""
                UPDATE "Invoices"
                SET "InvoiceNumber" = "InvoiceNumber" || '-DUP-' || "Id"
                WHERE "Id" IN (
                    SELECT "Id"
                    FROM (
                        SELECT "Id",
                               ROW_NUMBER() OVER (
                                   PARTITION BY "InvoiceNumber"
                                   ORDER BY "Id") AS "RowNumber"
                        FROM "Invoices"
                    )
                    WHERE "RowNumber" > 1
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber_Unique",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNumber_Unique",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "InvoiceDetails");
        }
    }
}
