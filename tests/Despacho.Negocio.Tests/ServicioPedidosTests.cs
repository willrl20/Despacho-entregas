using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Pedidos;
using Despacho.Negocio.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class ServicioPedidosTests
{
    // RF-NEG-02
    [Fact]
    public async Task RegistrarAsync_DatosValidos_ElPedidoNacePendiente()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };

        // Act
        var resultado = await servicio.RegistrarAsync(1, 1, 18.5, -69.8, "Casa azul", lineas);

        // Assert
        Assert.True(resultado.Exito);
        var pedido = await negocioDb.Pedidos.FindAsync(resultado.Valor);
        Assert.NotNull(pedido);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    }

    // RD-07
    [Fact]
    public async Task RegistrarAsync_SinLineas_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva>();

        // Act
        var resultado = await servicio.RegistrarAsync(1, 1, 18.5, -69.8, "Casa azul", lineas);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Empty(await negocioDb.Pedidos.ToListAsync());
    }

    // RF-NEG-02
    [Fact]
    public async Task CancelarAsync_PorElClienteDelPedido_QuedaCancelado()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        // Act
        var resultado = await servicio.CancelarAsync(5, false, registrado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Cancelado, pedido!.Estado);
    }

    // RF-NEG-02
    [Fact]
    public async Task CancelarAsync_PorAdministrador_QuedaCancelado()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        // Act
        var resultado = await servicio.CancelarAsync(99, true, registrado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Cancelado, pedido!.Estado);
    }

    // RF-NEG-06
    [Fact]
    public async Task CancelarAsync_PorOtroCliente_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        // Act
        var resultado = await servicio.CancelarAsync(99, false, registrado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal("Solo el cliente del pedido o un Administrador pueden cancelarlo.", resultado.Error);
        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Pendiente, pedido!.Estado);
    }

    // RF-NEG-06
    [Fact]
    public async Task MarcarEnRutaAsync_PorOtroRepartidor_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        pedido!.CambiarEstado(EstadoPedido.Asignado);
        pedido.RepartidorId = 10;
        await negocioDb.SaveChangesAsync();

        // Act
        var resultado = await servicio.MarcarEnRutaAsync(11, registrado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        var pedidoActualizado = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        Assert.Equal(EstadoPedido.Asignado, pedidoActualizado!.Estado);
    }

    // RF-NEG-02
    [Fact]
    public async Task VerDestinoAsync_RepartidorAsignado_DevuelveElDestino()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        var pedido = await negocioDb.Pedidos.FindAsync(registrado.Valor);
        pedido!.CambiarEstado(EstadoPedido.Asignado);
        pedido.RepartidorId = 10;
        await negocioDb.SaveChangesAsync();

        // Act
        var resultado = await servicio.VerDestinoAsync(10, registrado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(18.5, resultado.Valor!.Latitud);
        Assert.Equal(-69.8, resultado.Valor.Longitud);
        Assert.Equal("Casa azul", resultado.Valor.Referencia);
    }

    // RF-NEG-06
    [Fact]
    public async Task VerDestinoAsync_PedidoSinAsignar_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioPedidos(negocioDb, servicioPermisos, reloj);

        var lineas = new List<LineaNueva> { new LineaNueva("Pan", 2, 50m) };
        var registrado = await servicio.RegistrarAsync(5, 1, 18.5, -69.8, "Casa azul", lineas);

        // Act
        var resultado = await servicio.VerDestinoAsync(10, registrado.Valor);

        // Assert
        Assert.False(resultado.Exito);
    }
}
