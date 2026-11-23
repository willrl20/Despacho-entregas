using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ServicioReporteSolicitudesTests
{
    private static readonly UsuarioActual Administrador = new(1, "a1@test.com", Rol.Administrador);
    private static readonly UsuarioActual Repartidor = new(4, "repartidor@test.com", Rol.Estandar);

    private static CoreDbContext CrearBase()
    {
        return new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static SolicitudPermiso Solicitud(int solicitanteId, DateTime creadaEnUtc, EstadoSolicitud estado = EstadoSolicitud.Pendiente)
    {
        var solicitud = new SolicitudPermiso { SolicitanteId = solicitanteId, PermisoSolicitado = Permiso.Repartidor, CreadaEnUtc = creadaEnUtc };
        if (estado == EstadoSolicitud.Aplicada)
        {
            solicitud.Aprobar(1, creadaEnUtc);
            solicitud.MarcarAplicada();
        }
        else if (estado == EstadoSolicitud.Rechazada)
        {
            solicitud.Rechazar(1, "Motivo", creadaEnUtc);
        }

        return solicitud;
    }

    // C-21 RF-REP-01, RF-REP-03
    [Fact]
    public async Task ObtenerAsync_CincoSolicitudesDeTresEstados_DevuelveTresFilasConSuCantidad()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.SolicitudesPermiso.AddRange(
            Solicitud(4, dia),
            Solicitud(5, dia),
            Solicitud(4, dia, EstadoSolicitud.Aplicada),
            Solicitud(5, dia, EstadoSolicitud.Aplicada),
            Solicitud(4, dia, EstadoSolicitud.Rechazada));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-01", "2026-11-30");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(3, resultado.Valor!.Count);
        Assert.Equal(2, resultado.Valor.Single(f => f.Estado == "Pendiente").Cantidad);
        Assert.Equal(2, resultado.Valor.Single(f => f.Estado == "Aplicada").Cantidad);
        Assert.Equal(1, resultado.Valor.Single(f => f.Estado == "Rechazada").Cantidad);
    }

    // C-24 RF-REP-04
    [Fact]
    public async Task ObtenerAsync_UsuarioEstandar_SoloCuentaSusPropiasSolicitudes()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.SolicitudesPermiso.AddRange(
            Solicitud(4, dia),
            Solicitud(5, dia),
            Solicitud(5, dia));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Repartidor, "2026-11-01", "2026-11-30");

        // Assert
        Assert.True(resultado.Exito);
        var fila = Assert.Single(resultado.Valor!);
        Assert.Equal("Pendiente", fila.Estado);
        Assert.Equal(1, fila.Cantidad);
    }

    // C-25 RF-REP-05
    [Fact]
    public async Task ObtenerAsync_DosSemanasDistintas_CadaRangoCuentaSoloLoDeSuSemana()
    {
        // Arrange
        using var db = CrearBase();
        var primeraSemana = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);
        var segundaSemana = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.SolicitudesPermiso.AddRange(
            Solicitud(4, primeraSemana),
            Solicitud(5, segundaSemana),
            Solicitud(6, segundaSemana));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var primera = await servicio.ObtenerAsync(Administrador, "2026-11-02", "2026-11-08");
        var segunda = await servicio.ObtenerAsync(Administrador, "2026-11-09", "2026-11-15");

        // Assert
        Assert.Equal(1, Assert.Single(primera.Valor!).Cantidad);
        Assert.Equal(2, Assert.Single(segunda.Valor!).Cantidad);
    }

    // C-26 RF-REP-05, RD-11
    [Fact]
    public async Task ObtenerAsync_SolicitudDelUltimoDiaALas2330_SeCuenta()
    {
        // Arrange
        using var db = CrearBase();
        db.SolicitudesPermiso.Add(Solicitud(4, new DateTime(2026, 11, 20, 23, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-20", "2026-11-20");

        // Assert
        Assert.Equal(1, Assert.Single(resultado.Valor!).Cantidad);
    }

    // C-26 RF-REP-05, RD-11
    [Fact]
    public async Task ObtenerAsync_SolicitudDeMedianocheDelDiaSiguiente_NoSeCuenta()
    {
        // Arrange
        using var db = CrearBase();
        db.SolicitudesPermiso.Add(Solicitud(4, new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-20", "2026-11-20");

        // Assert
        Assert.Empty(resultado.Valor!);
    }

    // C-27 RD-07
    [Fact]
    public async Task ObtenerAsync_DesdePosteriorAHasta_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-20", "2026-11-10");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(RangoFechas.MensajeRangoInvertido, resultado.Error);
    }

    // C-27 RD-07
    [Fact]
    public async Task ObtenerAsync_FechaQueNoExiste_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-13-40", "2026-11-10");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(RangoFechas.MensajeFechaInvalida, resultado.Error);
    }

    // C-28 RF-REP-05
    [Fact]
    public async Task ObtenerAsync_RangoSinSolicitudes_DevuelveListaVacia()
    {
        // Arrange
        using var db = CrearBase();
        db.SolicitudesPermiso.Add(Solicitud(4, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteSolicitudes(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-01-01", "2026-01-31");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Empty(resultado.Valor!);
    }
}
