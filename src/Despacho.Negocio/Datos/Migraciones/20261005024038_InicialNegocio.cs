using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Despacho.Negocio.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class InicialNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TiendaId = table.Column<int>(type: "int", nullable: false),
                    DestinatarioId = table.Column<int>(type: "int", nullable: false),
                    RepartidorId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DestinatarioId",
                table: "Pedidos",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_TiendaId",
                table: "Pedidos",
                column: "TiendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedidos");
        }
    }
}
