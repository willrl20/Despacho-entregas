using Despacho.Core.Comun;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Despacho.Negocio.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class ReporteRepartidoresFechaEntregaTests
{
    private sealed class RelojAjustable : IReloj
    {
        public DateTime AhoraUtc { get; set; }
    }

    private static DatosEnvio DatosValidos() => new DatosEnvio(
        "Enviar",
        18.4861,
        -69.8571,
        "Casa azul",
        18.4700,
        -69.8900,
        "Oficina",
        "Ana Perez",
        "809-555-1234",
        "Documentos",
        "Pequeno");

    // Hallazgo 2 RF-REP-05
    [Fact]
    public async Task ObtenerAsync_EnvioCreadoAntesDelRangoYEntregadoDentro_CuentaPorLaFechaDeEntrega()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojAjustable { AhoraUtc = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc) };

        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 30, Correo = "cliente@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.PermisosConcedidos.Add(new PermisoConcedido { UsuarioId = 10, Permiso = Permiso.Repartidor, ConcedidoEnUtc = reloj.AhoraUtc });
        await coreDb.SaveChangesAsync();

        var notificaciones = new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj);
        var permisos = new ServicioPermisos(coreDb, reloj, notificaciones);
        var envios = new ServicioEnvios(negocioDb, permisos, reloj, notificaciones);
        var reporte = new ServicioReporteRepartidores(negocioDb);
        var administrador = new UsuarioActual(1, "admin@test.com", Rol.Administrador);

        var creado = await envios.CrearAsync(30, DatosValidos());
        reloj.AhoraUtc = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc);
        await envios.AceptarAsync(10, creado.Valor);
        await envios.MarcarRecogidoAsync(10, creado.Valor);
        await envios.ConfirmarEntregaAsync(10, creado.Valor);

        // Act
        var dia10 = await reporte.ObtenerAsync(administrador, "2026-11-10", "2026-11-10");
        var dia1 = await reporte.ObtenerAsync(administrador, "2026-11-01", "2026-11-01");

        // Assert
        Assert.True(dia10.Exito);
        var fila = Assert.Single(dia10.Valor!);
        Assert.Equal(10, fila.RepartidorId);
        Assert.Equal(1, fila.EnviosEntregados);
        Assert.True(dia1.Exito);
        Assert.Empty(dia1.Valor!);
    }
}
