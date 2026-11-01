using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Pedidos;
using Despacho.Negocio.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class IntegracionPermisosPedidosTests
{
    // Integración RF-NEG-02 con Gestión de permisos
    [Fact]
    public async Task AceptarAsync_ConSolicitudSinAprobar_SeRechazaYElPedidoSiguePendiente()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();

        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 20, Correo = "admin@test.com", Rol = Rol.Administrador, Activada = true });
        await coreDb.SaveChangesAsync();

        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(30, 1, 18.5, -69.8, "Casa azul", lineas);

        await servicioPermisos.CrearAsync(10, "Repartidor");

        // Act
        var resultado = await servicio.AceptarAsync(10, registrado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal("Necesitas el permiso Repartidor aprobado.", resultado.Error);
        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Pendiente, pedido!.Estado);
    }

    // Integración RF-NEG-02 con Gestión de permisos
    [Fact]
    public async Task AceptarAsync_DespuesDeAprobarElPermiso_ElPedidoQuedaAsignado()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();

        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 20, Correo = "admin@test.com", Rol = Rol.Administrador, Activada = true });
        await coreDb.SaveChangesAsync();

        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(30, 1, 18.5, -69.8, "Casa azul", lineas);

        var solicitud = await servicioPermisos.CrearAsync(10, "Repartidor");
        var aprobacion = await servicioPermisos.AprobarAsync(20, solicitud.Valor);
        Assert.True(aprobacion.Exito);

        // Act
        var resultado = await servicio.AceptarAsync(10, registrado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Asignado, pedido!.Estado);
        Assert.Equal(10, pedido.RepartidorId);
    }
}
