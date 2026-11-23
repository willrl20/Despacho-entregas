using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Despacho.Negocio.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class ServicioReporteRepartidoresTests
{
    private static readonly UsuarioActual Administrador = new(1, "a1@test.com", Rol.Administrador);

    private static NegocioDbContext CrearBase()
    {
        return new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static Envio Entregado(int repartidorId, DateTime creadoEnUtc, double km, decimal costo)
    {
        var envio = Nuevo(repartidorId, creadoEnUtc, km, costo);
        envio.CambiarEstado(EstadoEnvio.Asignado);
        envio.CambiarEstado(EstadoEnvio.EnRuta);
        envio.CambiarEstado(EstadoEnvio.Entregado);
        return envio;
    }

    private static Envio Nuevo(int? repartidorId, DateTime creadoEnUtc, double km, decimal costo)
    {
        return new Envio
        {
            ClienteId = 30,
            RepartidorId = repartidorId,
            CreadoEnUtc = creadoEnUtc,
            ContactoNombre = "Ana",
            ContactoTelefono = "809-555-1234",
            DescripcionPaquete = "Documentos",
            DistanciaKm = km,
            Costo = costo
        };
    }

    // C-22 RF-REP-02, RF-REP-03
    [Fact]
    public async Task ObtenerAsync_DosRepartidoresConEntregas_DevuelveUnaFilaPorRepartidorOrdenadaPorLoGenerado()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.Envios.AddRange(
            Entregado(10, dia, 2.0, 100m),
            Entregado(10, dia, 3.0, 150m),
            Entregado(10, dia, 5.0, 250m),
            Entregado(11, dia, 4.0, 300m),
            Entregado(11, dia, 6.0, 400m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-01", "2026-11-30");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(2, resultado.Valor!.Count);
        Assert.Equal(new FilaReporteRepartidores(11, 2, 10.0, 700m), resultado.Valor[0]);
        Assert.Equal(new FilaReporteRepartidores(10, 3, 10.0, 500m), resultado.Valor[1]);
    }

    // C-22 RF-REP-02
    [Fact]
    public async Task ObtenerAsync_EnviosQueNoEstanEntregados_NoSeCuentan()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        var enRuta = Nuevo(10, dia, 5.0, 200m);
        enRuta.CambiarEstado(EstadoEnvio.Asignado);
        enRuta.CambiarEstado(EstadoEnvio.EnRuta);
        db.Envios.AddRange(Entregado(10, dia, 2.0, 100m), enRuta, Nuevo(null, dia, 1.0, 50m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-01", "2026-11-30");

        // Assert
        var fila = Assert.Single(resultado.Valor!);
        Assert.Equal(1, fila.EnviosEntregados);
        Assert.Equal(100m, fila.TotalGenerado);
    }

    // C-24 RF-REP-04
    [Fact]
    public async Task ObtenerAsync_UsuarioEstandar_SoloVeSuPropiaFila()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.Envios.AddRange(Entregado(10, dia, 2.0, 100m), Entregado(11, dia, 4.0, 300m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(new UsuarioActual(10, "r@test.com", Rol.Estandar), "2026-11-01", "2026-11-30");

        // Assert
        var fila = Assert.Single(resultado.Valor!);
        Assert.Equal(10, fila.RepartidorId);
    }

    // C-25 RF-REP-05
    [Fact]
    public async Task ObtenerAsync_DosSemanasDistintas_CadaRangoCuentaSoloLoDeSuSemana()
    {
        // Arrange
        using var db = CrearBase();
        db.Envios.AddRange(
            Entregado(10, new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc), 2.0, 100m),
            Entregado(10, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc), 3.0, 150m),
            Entregado(10, new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc), 5.0, 250m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var primera = await servicio.ObtenerAsync(Administrador, "2026-11-02", "2026-11-08");
        var segunda = await servicio.ObtenerAsync(Administrador, "2026-11-09", "2026-11-15");

        // Assert
        Assert.Equal(1, Assert.Single(primera.Valor!).EnviosEntregados);
        Assert.Equal(2, Assert.Single(segunda.Valor!).EnviosEntregados);
    }

    // C-26 RF-REP-05, RD-11
    [Fact]
    public async Task ObtenerAsync_EnvioDelUltimoDiaALas2330_SeCuenta()
    {
        // Arrange
        using var db = CrearBase();
        db.Envios.Add(Entregado(10, new DateTime(2026, 11, 20, 23, 30, 0, DateTimeKind.Utc), 2.0, 100m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-20", "2026-11-20");

        // Assert
        Assert.Equal(1, Assert.Single(resultado.Valor!).EnviosEntregados);
    }

    // C-27 RD-07
    [Fact]
    public async Task ObtenerAsync_DesdePosteriorAHasta_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioReporteRepartidores(db);

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
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-11-10", "2026-13-40");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(RangoFechas.MensajeFechaInvalida, resultado.Error);
    }

    // C-28 RF-REP-05
    [Fact]
    public async Task ObtenerAsync_RangoSinEnvios_DevuelveListaVacia()
    {
        // Arrange
        using var db = CrearBase();
        db.Envios.Add(Entregado(10, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc), 2.0, 100m));
        await db.SaveChangesAsync();
        var servicio = new ServicioReporteRepartidores(db);

        // Act
        var resultado = await servicio.ObtenerAsync(Administrador, "2026-01-01", "2026-01-31");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Empty(resultado.Valor!);
    }
}
