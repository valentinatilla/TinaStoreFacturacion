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
            // Antes de crear los índices únicos, resolver duplicados activos en Name y Sku
            // para que la migración no falle en equipos con datos previos.
            // Duplicados de Name: se añade " (Id)" al nombre del duplicado.
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

            // Duplicados de Sku (solo filas activas con Sku no nulo y no vacío):
            // se añade "-Id" al Sku del duplicado.
            migrationBuilder.Sql("""
                UPDATE Products
                SET Sku = Sku || '-' || Id
                WHERE IsDeleted = 0
                  AND Sku IS NOT NULL
                  AND Sku != ''
                  AND Sku IN (
                      SELECT Sku
                      FROM Products
                      WHERE IsDeleted = 0
                        AND Sku IS NOT NULL
                        AND Sku != ''
                      GROUP BY Sku
                      HAVING COUNT(*) > 1
                  )
                  AND Id NOT IN (
                      SELECT MIN(Id)
                      FROM Products
                      WHERE IsDeleted = 0
                        AND Sku IS NOT NULL
                        AND Sku != ''
                      GROUP BY Sku
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
