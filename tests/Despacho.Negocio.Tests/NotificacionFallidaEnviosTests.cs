using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Despacho.Negocio.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class NotificacionFallidaEnviosTests
{
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

    // Hallazgo 3 RF-NOT-08
    [Fact]
    public async Task AceptarAsync_FallaLaNotificacion_ElEnvioSeAceptaIgual()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 30, Correo = "cliente@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.PermisosConcedidos.Add(new PermisoConcedido { UsuarioId = 10, Permiso = Permiso.Repartidor, ConcedidoEnUtc = RelojFijo.Ahora });
        await coreDb.SaveChangesAsync();

        var notificaciones = new NotificacionesQueFallan();
        var permisos = new ServicioPermisos(coreDb, reloj, notificaciones);
        var servicio = new ServicioEnvios(negocioDb, permisos, reloj, notificaciones);
        var creado = await servicio.CrearAsync(30, DatosValidos());

        // Act
        Despacho.Core.Comun.Resultado? resultado = null;
        var excepcion = await Record.ExceptionAsync(async () => resultado = await servicio.AceptarAsync(10, creado.Valor));

        // Assert
        Assert.Null(excepcion);
        Assert.True(resultado!.Exito);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Asignado, envio!.Estado);
        Assert.Equal(10, envio.RepartidorId);
    }
}
