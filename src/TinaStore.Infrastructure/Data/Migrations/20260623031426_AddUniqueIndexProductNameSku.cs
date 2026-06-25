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
            // Antes de crear el índice único, renombrar productos activos duplicados
            // para que la migración no falle en equipos con datos previos.
            // Se añade " (N)" al nombre del duplicado, donde N es su Id.
            migrationBuilder.Sql("""
                UPDATE Products
                SET Name = Name || ' (' || Id || ')'
                WHERE IsDeleted = 0
                  AND Name IN (
                      SELECT Name
                      FROM Products
                      WHERE IsDeleted = 0
                      GROUP BY Name
                      HAVING COUNT(*) > 1
                  )
                  AND Id NOT IN (
                      SELECT MIN(Id)
                      FROM Products
                      WHERE IsDeleted = 0
                      GROUP BY Name
                      HAVING COUNT(*) > 1
                  );
                """);

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
