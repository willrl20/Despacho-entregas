using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Core.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ReservaCorreos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservadoHastaUtc",
                table: "CorreosEnCola",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservadoHastaUtc",
                table: "CorreosEnCola");
        }
    }
}
