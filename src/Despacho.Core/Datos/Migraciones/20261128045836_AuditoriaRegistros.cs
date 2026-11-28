using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Core.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AuditoriaRegistros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForzadoPorAdministradorId",
                table: "CodigosRecuperacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    ValorNuevo = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_Entidad",
                table: "RegistrosAuditoria",
                column: "Entidad");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_FechaUtc",
                table: "RegistrosAuditoria",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_UsuarioId",
                table: "RegistrosAuditoria",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropColumn(
                name: "ForzadoPorAdministradorId",
                table: "CodigosRecuperacion");
        }
    }
}
