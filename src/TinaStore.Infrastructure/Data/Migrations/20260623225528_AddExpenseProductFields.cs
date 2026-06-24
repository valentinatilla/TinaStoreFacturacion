using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinaStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Expenses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockQty",
                table: "Expenses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ProductId",
                table: "Expenses",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Products_ProductId",
                table: "Expenses",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Products_ProductId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ProductId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "StockQty",
                table: "Expenses");
        }
    }
}
