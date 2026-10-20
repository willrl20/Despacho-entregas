using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ServicioPermisosTests
{
    [Fact]
    public async Task AprobarAsync_SolicitudPendiente_QuedaAplicadaConAprobadorYFecha()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        var aprobador = new Usuario { Id = 2, Correo = "aprobador@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        db.Usuarios.AddRange(solicitante, aprobador);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);

        // Act
        var resultado = await servicio.AprobarAsync(aprobador.Id, solicitud.Id);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(EstadoSolicitud.Aplicada, solicitud.Estado);
        Assert.Equal(aprobador.Id, solicitud.AprobadorId);
        Assert.Equal(RelojFijo.Ahora, solicitud.ResueltaEnUtc);
    }

    [Fact]
    public async Task AprobarAsync_SolicitudPendiente_CambiaElRolDelSolicitante()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        var aprobador = new Usuario { Id = 2, Correo = "aprobador@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        db.Usuarios.AddRange(solicitante, aprobador);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);

        // Act
        await servicio.AprobarAsync(aprobador.Id, solicitud.Id);

        // Assert
        Assert.Equal(Rol.Administrador, solicitante.Rol);
    }

    [Fact]
    public async Task AprobarAsync_SolicitudYaAprobada_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        var aprobador1 = new Usuario { Id = 2, Correo = "aprobador1@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        var aprobador2 = new Usuario { Id = 3, Correo = "aprobador2@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        db.Usuarios.AddRange(solicitante, aprobador1, aprobador2);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);
        await servicio.AprobarAsync(aprobador1.Id, solicitud.Id);

        // Act
        var resultado = await servicio.AprobarAsync(aprobador2.Id, solicitud.Id);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(EstadoSolicitud.Aplicada, solicitud.Estado);
        Assert.Equal(aprobador1.Id, solicitud.AprobadorId);
    }

    [Fact]
    public async Task AprobarAsync_SolicitudPropia_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        db.Usuarios.Add(solicitante);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);

        // Act
        var resultado = await servicio.AprobarAsync(solicitante.Id, solicitud.Id);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal("No puedes resolver tu propia solicitud.", resultado.Error);
        Assert.Equal(EstadoSolicitud.Pendiente, solicitud.Estado);
    }

    [Fact]
    public async Task RechazarAsync_ConMotivo_RegistraMotivoYFecha()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        var aprobador = new Usuario { Id = 2, Correo = "aprobador@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        db.Usuarios.AddRange(solicitante, aprobador);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);
        var motivo = "No cumple los requisitos mínimos";

        // Act
        var resultado = await servicio.RechazarAsync(aprobador.Id, solicitud.Id, motivo);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(EstadoSolicitud.Rechazada, solicitud.Estado);
        Assert.Equal(motivo, solicitud.MotivoRechazo);
        Assert.Equal(RelojFijo.Ahora, solicitud.ResueltaEnUtc);
        Assert.Equal(aprobador.Id, solicitud.AprobadorId);
    }

    [Fact]
    public async Task RechazarAsync_SinMotivo_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using var db = new CoreDbContext(options);
        var solicitante = new Usuario { Id = 1, Correo = "solicitante@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar };
        var aprobador = new Usuario { Id = 2, Correo = "aprobador@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador };
        db.Usuarios.AddRange(solicitante, aprobador);

        var solicitud = new SolicitudPermiso { SolicitanteId = 1, PermisoSolicitado = Rol.Administrador, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        var servicio = new ServicioPermisos(db, reloj);

        // Act
        var resultado = await servicio.RechazarAsync(aprobador.Id, solicitud.Id, null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal("Escribe el motivo del rechazo.", resultado.Error);
        Assert.Equal(EstadoSolicitud.Pendiente, solicitud.Estado);
    }
}
