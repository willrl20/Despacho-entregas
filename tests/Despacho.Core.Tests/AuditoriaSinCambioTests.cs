using Despacho.Core.Administracion;
using Despacho.Core.Datos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class AuditoriaSinCambioTests
{
    private const int AdministradorId = 1;
    private const int UsuarioId = 2;

    private static async Task<CoreDbContext> CrearBaseConUsuarios(bool desactivado = false)
    {
        var db = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Usuarios.Add(new Usuario { Id = AdministradorId, Correo = "a1@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador });
        db.Usuarios.Add(new Usuario { Id = UsuarioId, Correo = "r@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar, Desactivado = desactivado });
        await db.SaveChangesAsync();
        return db;
    }

    // A-06 RF-AUD-04
    [Fact]
    public async Task ReactivarAsync_UsuarioQueYaEstaActivo_NoCreaRegistroDeAuditoria()
    {
        // Arrange
        using var db = await CrearBaseConUsuarios();
        var servicio = new ServicioAdministracion(db, new RelojFijo());

        // Act
        await servicio.ReactivarAsync(AdministradorId, UsuarioId);

        // Assert
        Assert.Empty(await db.RegistrosAuditoria.ToListAsync());
    }

    // A-05 RF-AUD-04
    [Fact]
    public async Task CambiarRolAsync_ElMismoRolQueYaTiene_NoCreaRegistroDeAuditoria()
    {
        // Arrange
        using var db = await CrearBaseConUsuarios();
        var servicio = new ServicioAdministracion(db, new RelojFijo());

        // Act
        await servicio.CambiarRolAsync(AdministradorId, UsuarioId, "Estandar");

        // Assert
        Assert.Empty(await db.RegistrosAuditoria.ToListAsync());
    }

    // A-06 RF-AUD-04
    [Fact]
    public async Task DesactivarAsync_UsuarioQueYaEstaDesactivado_NoCreaRegistroDeAuditoria()
    {
        // Arrange
        using var db = await CrearBaseConUsuarios(desactivado: true);
        var servicio = new ServicioAdministracion(db, new RelojFijo());

        // Act
        await servicio.DesactivarAsync(AdministradorId, UsuarioId);

        // Assert
        Assert.Empty(await db.RegistrosAuditoria.ToListAsync());
    }
}
