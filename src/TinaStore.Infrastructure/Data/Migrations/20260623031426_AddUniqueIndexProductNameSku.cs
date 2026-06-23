using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinaStore.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexProductNameSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_Unique",
                table: "Products",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku_Unique",
                table: "Products",
                column: "Sku",
                unique: true,
                filter: "\"IsDeleted\" = 0 AND \"Sku\" IS NOT NULL AND \"Sku\" != ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name_Unique",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku_Unique",
                table: "Products");
        }
    }
}
