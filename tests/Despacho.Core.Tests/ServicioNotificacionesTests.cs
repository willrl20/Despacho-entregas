using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ServicioNotificacionesTests
{
    private static CoreDbContext CrearBase()
    {
        return new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static Usuario AgregarUsuario(CoreDbContext db, int id, string correo, Rol rol = Rol.Estandar, bool activada = true, bool desactivado = false)
    {
        var usuario = new Usuario { Id = id, Correo = correo, HashContrasena = "hash", Activada = activada, Desactivado = desactivado, Rol = rol };
        db.Usuarios.Add(usuario);
        return usuario;
    }

    private static ServicioNotificaciones CrearNotificaciones(CoreDbContext db, RelojFijo reloj)
    {
        return new ServicioNotificaciones(db, new ColaCorreos(db, reloj), reloj);
    }

    private static ServicioPermisos CrearPermisos(CoreDbContext db, RelojFijo reloj)
    {
        return new ServicioPermisos(db, reloj, CrearNotificaciones(db, reloj));
    }

    private static async Task<SolicitudPermiso> AgregarSolicitudPendiente(CoreDbContext db, int solicitanteId, Permiso permiso)
    {
        var solicitud = new SolicitudPermiso { SolicitanteId = solicitanteId, PermisoSolicitado = permiso, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();
        return solicitud;
    }

    // C-01 RF-NOT-01
    [Fact]
    public async Task CrearAsync_SolicitudNueva_AvisaALosAdministradoresActivos()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 2, "a2@test.com", Rol.Administrador);
        AgregarUsuario(db, 3, "a3@test.com", Rol.Administrador, desactivado: true);
        AgregarUsuario(db, 4, "repartidor@test.com");
        AgregarUsuario(db, 5, "cliente@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearPermisos(db, reloj);

        // Act
        var resultado = await servicio.CrearAsync(4, "Repartidor");

        // Assert
        Assert.True(resultado.Exito);
        var notificaciones = await db.Notificaciones.ToListAsync();
        Assert.Equal(new[] { 1, 2 }, notificaciones.Select(n => n.DestinatarioId).OrderBy(i => i).ToArray());
        Assert.All(notificaciones, n =>
        {
            Assert.False(n.Leida);
            Assert.Contains("repartidor@test.com", n.Mensaje);
            Assert.Contains("Repartidor", n.Mensaje);
        });
    }

    // C-02 RF-NOT-02
    [Fact]
    public async Task AprobarAsync_SolicitudPendiente_AvisaSoloAlSolicitanteQueFueAprobada()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 2, "a2@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var solicitud = await AgregarSolicitudPendiente(db, 4, Permiso.Repartidor);
        var servicio = CrearPermisos(db, reloj);

        // Act
        var resultado = await servicio.AprobarAsync(1, solicitud.Id);

        // Assert
        Assert.True(resultado.Exito);
        var notificacion = Assert.Single(await db.Notificaciones.ToListAsync());
        Assert.Equal(4, notificacion.DestinatarioId);
        Assert.False(notificacion.Leida);
        Assert.Contains("aprobada", notificacion.Mensaje);
    }

    // C-03 RF-NOT-02
    [Fact]
    public async Task RechazarAsync_ConMotivo_AvisaAlSolicitanteQueFueRechazadaConElMotivo()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var solicitud = await AgregarSolicitudPendiente(db, 4, Permiso.Repartidor);
        var servicio = CrearPermisos(db, reloj);

        // Act
        var resultado = await servicio.RechazarAsync(1, solicitud.Id, "Falta la licencia");

        // Assert
        Assert.True(resultado.Exito);
        var notificacion = Assert.Single(await db.Notificaciones.ToListAsync());
        Assert.Equal(4, notificacion.DestinatarioId);
        Assert.False(notificacion.Leida);
        Assert.Contains("rechazada", notificacion.Mensaje);
        Assert.Contains("Falta la licencia", notificacion.Mensaje);
    }

    // C-06 RF-NOT-04
    [Fact]
    public async Task ListarAsync_ConNotificacionesDeVariosUsuarios_DevuelveSoloLasDelUsuario()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);
        await servicio.NotificarAsync(1, "Para el administrador", "Mensaje del administrador");
        await servicio.NotificarAsync(4, "Para el repartidor", "Mensaje del repartidor");

        // Act
        var lista = await servicio.ListarAsync(4, false);

        // Assert
        var propia = Assert.Single(lista);
        Assert.Equal("Para el repartidor", propia.Titulo);
    }

    // C-06 RF-NOT-04
    [Fact]
    public async Task ListarAsync_VariasNotificaciones_LasOrdenaDeLaMasNuevaALaMasVieja()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 4, "repartidor@test.com");
        db.Notificaciones.Add(new Notificacion { DestinatarioId = 4, Titulo = "Vieja", Mensaje = "m", CreadaEnUtc = RelojFijo.Ahora.AddHours(-2) });
        db.Notificaciones.Add(new Notificacion { DestinatarioId = 4, Titulo = "Nueva", Mensaje = "m", CreadaEnUtc = RelojFijo.Ahora });
        db.Notificaciones.Add(new Notificacion { DestinatarioId = 4, Titulo = "Media", Mensaje = "m", CreadaEnUtc = RelojFijo.Ahora.AddHours(-1) });
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);

        // Act
        var lista = await servicio.ListarAsync(4, false);

        // Assert
        Assert.Equal(new[] { "Nueva", "Media", "Vieja" }, lista.Select(n => n.Titulo).ToArray());
    }

    // C-07 RF-NOT-04
    [Fact]
    public async Task ListarAsync_UnaLeidaYUnaNoLeida_LasMuestraConSuEstado()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);
        await servicio.NotificarAsync(4, "Primera", "m");
        await servicio.NotificarAsync(4, "Segunda", "m");
        var primera = await db.Notificaciones.SingleAsync(n => n.Titulo == "Primera");
        await servicio.MarcarLeidaAsync(4, primera.Id);

        // Act
        var lista = await servicio.ListarAsync(4, false);

        // Assert
        Assert.Equal(2, lista.Count);
        Assert.True(lista.Single(n => n.Titulo == "Primera").Leida);
        Assert.False(lista.Single(n => n.Titulo == "Segunda").Leida);
    }

    // C-07 RF-NOT-04
    [Fact]
    public async Task ListarAsync_SoloNoLeidas_OmiteLasLeidas()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);
        await servicio.NotificarAsync(4, "Primera", "m");
        await servicio.NotificarAsync(4, "Segunda", "m");
        var primera = await db.Notificaciones.SingleAsync(n => n.Titulo == "Primera");
        await servicio.MarcarLeidaAsync(4, primera.Id);

        // Act
        var lista = await servicio.ListarAsync(4, true);

        // Assert
        var noLeida = Assert.Single(lista);
        Assert.Equal("Segunda", noLeida.Titulo);
        Assert.False(noLeida.Leida);
    }

    // C-08 RF-NOT-05
    [Fact]
    public async Task MarcarLeidaAsync_NotificacionPropia_QuedaLeida()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);
        await servicio.NotificarAsync(4, "Aviso", "m");
        var notificacion = await db.Notificaciones.SingleAsync();

        // Act
        var resultado = await servicio.MarcarLeidaAsync(4, notificacion.Id);

        // Assert
        Assert.True(resultado.Exito);
        var lista = await servicio.ListarAsync(4, false);
        Assert.True(Assert.Single(lista).Leida);
    }

    // C-09 RF-NOT-05, RD-06
    [Fact]
    public async Task MarcarLeidaAsync_NotificacionDeOtro_FallaComoSiNoExistieraYNoLaMarca()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);
        await servicio.NotificarAsync(1, "Aviso", "m");
        var notificacion = await db.Notificaciones.SingleAsync();

        // Act
        var resultado = await servicio.MarcarLeidaAsync(4, notificacion.Id);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioNotificaciones.MensajeNoExiste, resultado.Error);
        Assert.False((await db.Notificaciones.SingleAsync()).Leida);
    }

    // C-10 RF-NOT-06
    [Fact]
    public async Task CrearAsync_AdministradorConCorreoActivo_RecibeNotificacionYCorreoConElMismoAsunto()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearPermisos(db, reloj);

        // Act
        await servicio.CrearAsync(4, "Repartidor");

        // Assert
        var notificacion = await db.Notificaciones.SingleAsync(n => n.DestinatarioId == 1);
        var correo = await db.CorreosEnCola.SingleAsync(c => c.Destinatario == "a1@test.com");
        Assert.Equal(notificacion.Titulo, correo.Asunto);
    }

    // C-11 RF-NOT-07
    [Fact]
    public async Task AprobarAsync_ConCorreoApagado_NotificaPeroNoEncolaCorreo()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var solicitud = await AgregarSolicitudPendiente(db, 4, Permiso.Repartidor);
        await CrearNotificaciones(db, reloj).CambiarPreferenciaCorreoAsync(4, false);
        var servicio = CrearPermisos(db, reloj);

        // Act
        await servicio.AprobarAsync(1, solicitud.Id);

        // Assert
        Assert.Single(await db.Notificaciones.Where(n => n.DestinatarioId == 4).ToListAsync());
        Assert.Empty(await db.CorreosEnCola.Where(c => c.Destinatario == "repartidor@test.com").ToListAsync());
    }

    // C-11 RF-NOT-07
    [Fact]
    public async Task AprobarAsync_CorreoApagadoYLuegoEncendido_ElSiguienteEventoEncolaCorreo()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 1, "a1@test.com", Rol.Administrador);
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var solicitud = await AgregarSolicitudPendiente(db, 4, Permiso.Repartidor);
        var notificaciones = CrearNotificaciones(db, reloj);
        await notificaciones.CambiarPreferenciaCorreoAsync(4, false);
        await notificaciones.CambiarPreferenciaCorreoAsync(4, true);
        var servicio = CrearPermisos(db, reloj);

        // Act
        await servicio.AprobarAsync(1, solicitud.Id);

        // Assert
        Assert.Single(await db.CorreosEnCola.Where(c => c.Destinatario == "repartidor@test.com").ToListAsync());
    }

    // C-13 RD-07
    [Fact]
    public async Task MarcarLeidaAsync_IdInexistente_FallaConMensajeClaroSinLanzarExcepcion()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        AgregarUsuario(db, 4, "repartidor@test.com");
        await db.SaveChangesAsync();
        var servicio = CrearNotificaciones(db, reloj);

        // Act
        var resultado = await servicio.MarcarLeidaAsync(4, 999999);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioNotificaciones.MensajeNoExiste, resultado.Error);
    }
}
