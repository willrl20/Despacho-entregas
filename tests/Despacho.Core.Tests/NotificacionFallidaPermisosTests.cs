using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class NotificacionFallidaPermisosTests
{
    // Hallazgo 3 RF-NOT-08
    [Fact]
    public async Task AprobarAsync_FallaLaNotificacion_LaSolicitudSeApruebaIgual()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        db.Usuarios.Add(new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar });
        db.Usuarios.Add(new Usuario { Id = 2, Correo = "aprobador@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador });
        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Permiso.Repartidor, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj, new NotificacionesQueFallan());

        // Act
        Resultado? resultado = null;
        var excepcion = await Record.ExceptionAsync(async () => resultado = await servicio.AprobarAsync(2, solicitud.Id));

        // Assert
        Assert.Null(excepcion);
        Assert.True(resultado!.Exito);
        Assert.Equal(EstadoSolicitud.Aplicada, solicitud.Estado);
        Assert.Equal(2, solicitud.AprobadorId);
    }
}
