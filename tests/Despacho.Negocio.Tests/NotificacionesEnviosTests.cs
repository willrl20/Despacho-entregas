using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Despacho.Negocio.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class NotificacionesEnviosTests
{
    private const int ClienteId = 30;
    private const int RepartidorId = 10;

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

    private static async Task SembrarUsuarios(CoreDbContext coreDb)
    {
        coreDb.Usuarios.Add(new Usuario { Id = RepartidorId, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = ClienteId, Correo = "cliente@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.PermisosConcedidos.Add(new PermisoConcedido { UsuarioId = RepartidorId, Permiso = Permiso.Repartidor, ConcedidoEnUtc = RelojFijo.Ahora });
        await coreDb.SaveChangesAsync();
    }

    // C-04 RF-NOT-03
    [Fact]
    public async Task AceptarAsync_EnvioPendiente_AvisaAlClienteConElNumeroYNoAlRepartidor()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        await SembrarUsuarios(coreDb);
        var notificaciones = new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj);
        var permisos = new ServicioPermisos(coreDb, reloj, notificaciones);
        var servicio = new ServicioEnvios(negocioDb, permisos, reloj, notificaciones);
        var creado = await servicio.CrearAsync(ClienteId, DatosValidos());

        // Act
        var resultado = await servicio.AceptarAsync(RepartidorId, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var notificacion = Assert.Single(await coreDb.Notificaciones.ToListAsync());
        Assert.Equal(ClienteId, notificacion.DestinatarioId);
        Assert.False(notificacion.Leida);
        Assert.Contains($"#{creado.Valor}", notificacion.Mensaje);
        Assert.Contains("aceptado", notificacion.Mensaje);
    }

    // C-05 RF-NOT-03
    [Fact]
    public async Task ConfirmarEntregaAsync_EnvioEnRuta_AvisaAlClienteQueFueEntregado()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        await SembrarUsuarios(coreDb);
        var notificaciones = new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj);
        var permisos = new ServicioPermisos(coreDb, reloj, notificaciones);
        var servicio = new ServicioEnvios(negocioDb, permisos, reloj, notificaciones);
        var creado = await servicio.CrearAsync(ClienteId, DatosValidos());
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        envio!.CambiarEstado(EstadoEnvio.Asignado);
        envio.CambiarEstado(EstadoEnvio.EnRuta);
        envio.RepartidorId = RepartidorId;
        await negocioDb.SaveChangesAsync();

        // Act
        var resultado = await servicio.ConfirmarEntregaAsync(RepartidorId, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var notificacion = Assert.Single(await coreDb.Notificaciones.ToListAsync());
        Assert.Equal(ClienteId, notificacion.DestinatarioId);
        Assert.False(notificacion.Leida);
        Assert.Contains("entregado", notificacion.Mensaje);
    }
}
