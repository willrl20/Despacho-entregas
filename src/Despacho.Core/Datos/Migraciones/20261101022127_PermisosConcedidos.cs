using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Core.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PermisosConcedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermisosConcedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Permiso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConcedidoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosConcedidos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermisosConcedidos_UsuarioId_Permiso",
                table: "PermisosConcedidos",
                columns: new[] { "UsuarioId", "Permiso" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermisosConcedidos");
        }
    }
}
