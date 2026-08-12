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
