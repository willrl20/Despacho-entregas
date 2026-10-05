using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Core.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Administracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Desactivado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desactivado",
                table: "Usuarios");
        }
    }
}
