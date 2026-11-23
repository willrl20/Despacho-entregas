using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Core.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class EstadosCola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "CorreosEnCola",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<int>(
                name: "Intentos",
                table: "CorreosEnCola",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UltimoError",
                table: "CorreosEnCola",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoIntentoEnUtc",
                table: "CorreosEnCola",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE CorreosEnCola SET Estado = 'Enviado', Intentos = 1, UltimoIntentoEnUtc = EnviadoEnUtc WHERE Enviado = 1");

            migrationBuilder.DropIndex(name: "IX_CorreosEnCola_Enviado", table: "CorreosEnCola");

            migrationBuilder.DropColumn(name: "Enviado", table: "CorreosEnCola");

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEnCola_Estado",
                table: "CorreosEnCola",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CorreosEnCola_Estado",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "Intentos",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "UltimoError",
                table: "CorreosEnCola");

            migrationBuilder.DropColumn(
                name: "UltimoIntentoEnUtc",
                table: "CorreosEnCola");

            migrationBuilder.AddColumn<bool>(
                name: "Enviado",
                table: "CorreosEnCola",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CorreosEnCola_Enviado",
                table: "CorreosEnCola",
                column: "Enviado");
        }
    }
}
