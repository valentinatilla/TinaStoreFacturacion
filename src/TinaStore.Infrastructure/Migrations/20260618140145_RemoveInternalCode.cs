using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinaStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInternalCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Migración de datos: copiar InternalCode → Sku donde Sku esté vacío ──
            // Esto preserva el identificador de los productos que solo tenían InternalCode.
            migrationBuilder.Sql(@"
                UPDATE Products
                SET Sku = InternalCode
                WHERE (Sku IS NULL OR Sku = '')
                  AND InternalCode IS NOT NULL
                  AND InternalCode != '';
            ");

            // ── Eliminar la columna ya no necesaria ───────────────────────────────
            migrationBuilder.DropColumn(
                name: "InternalCode",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalCode",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
