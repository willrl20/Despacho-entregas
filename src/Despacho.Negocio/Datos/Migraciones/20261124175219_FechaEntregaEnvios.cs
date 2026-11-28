using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Negocio.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class FechaEntregaEnvios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EntregadoEnUtc",
                table: "Envios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE Envios SET EntregadoEnUtc = CreadoEnUtc WHERE Estado = 'Entregado'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntregadoEnUtc",
                table: "Envios");
        }
    }
}
