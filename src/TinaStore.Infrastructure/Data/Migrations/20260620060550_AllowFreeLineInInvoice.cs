using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinaStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowFreeLineInInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Products_ProductId",
                table: "InvoiceDetails");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoiceDetails",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Products_ProductId",
                table: "InvoiceDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Products_ProductId",
                table: "InvoiceDetails");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoiceDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Products_ProductId",
                table: "InvoiceDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
