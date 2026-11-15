using Despacho.Core.Datos;
using Despacho.Core.Permisos;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Despacho.Negocio.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Tests;

public class ServicioEnviosTests
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

    // RF-NEG-02
    [Fact]
    public async Task CrearAsync_DatosValidos_ElEnvioNacePendienteConDistanciaYCosto()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        // Act
        var resultado = await servicio.CrearAsync(1, DatosValidos());

        // Assert
        Assert.True(resultado.Exito);
        var envio = await negocioDb.Envios.FindAsync(resultado.Valor);
        Assert.NotNull(envio);
        Assert.Equal(EstadoEnvio.Pendiente, envio.Estado);
        Assert.True(envio.DistanciaKm > 0);
        Assert.Equal(TarifaEnvio.CalcularCosto(envio.DistanciaKm, TamanoPaquete.Pequeno), envio.Costo);
    }

    // RF-NEG-02
    [Fact]
    public async Task CrearAsync_ModalidadRecibir_GuardaLaModalidad()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var datos = DatosValidos() with { Modalidad = "Recibir" };

        // Act
        var resultado = await servicio.CrearAsync(1, datos);

        // Assert
        Assert.True(resultado.Exito);
        var envio = await negocioDb.Envios.FindAsync(resultado.Valor);
        Assert.Equal(ModalidadEnvio.Recibir, envio!.Modalidad);
    }

    // RD-07
    [Fact]
    public async Task CrearAsync_MismoPuntoDeRecogidaYEntrega_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var original = DatosValidos();
        var datos = original with { DestinoLatitud = original.OrigenLatitud, DestinoLongitud = original.OrigenLongitud };

        // Act
        var resultado = await servicio.CrearAsync(1, datos);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Empty(await negocioDb.Envios.ToListAsync());
    }

    // RD-07
    [Fact]
    public async Task CrearAsync_SinContacto_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var datos = DatosValidos() with { ContactoNombre = "" };

        // Act
        var resultado = await servicio.CrearAsync(1, datos);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeDatosInvalidos, resultado.Error);
    }

    // RD-07
    [Fact]
    public async Task CrearAsync_ModalidadInvalida_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var datos = DatosValidos() with { Modalidad = "Volar" };

        // Act
        var resultado = await servicio.CrearAsync(1, datos);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeDatosInvalidos, resultado.Error);
    }

    // RD-07
    [Fact]
    public void Cotizar_MasDe30Km_SeRechazaPorDistancia()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var datos = DatosValidos() with { DestinoLatitud = 19.4500, DestinoLongitud = -70.7000 };

        // Act
        var resultado = servicio.Cotizar(datos);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeMuyLejos, resultado.Error);
    }

    // RF-NEG-02
    [Fact]
    public async Task CancelarAsync_PorElClienteDelEnvio_QuedaCancelado()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());

        // Act
        var resultado = await servicio.CancelarAsync(5, false, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Cancelado, envio!.Estado);
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
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());

        // Act
        var resultado = await servicio.CancelarAsync(99, true, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Cancelado, envio!.Estado);
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
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());

        // Act
        var resultado = await servicio.CancelarAsync(6, false, creado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeNoPuedeCancelar, resultado.Error);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Pendiente, envio!.Estado);
    }

    // RF-NEG-06
    [Fact]
    public async Task MarcarRecogidoAsync_PorOtroRepartidor_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        envio!.CambiarEstado(EstadoEnvio.Asignado);
        envio.RepartidorId = 10;
        await negocioDb.SaveChangesAsync();

        // Act
        var resultado = await servicio.MarcarRecogidoAsync(11, creado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeNoEsTuEnvio, resultado.Error);
        Assert.Equal(EstadoEnvio.Asignado, envio.Estado);
    }

    // RF-NEG-02
    [Fact]
    public async Task VerRutaAsync_RepartidorAsignado_DevuelveLosDosPuntosYElContacto()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        envio!.CambiarEstado(EstadoEnvio.Asignado);
        envio.RepartidorId = 10;
        await negocioDb.SaveChangesAsync();

        // Act
        var resultado = await servicio.VerRutaAsync(10, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var ruta = resultado.Valor!;
        Assert.Equal(18.4861, ruta.OrigenLatitud);
        Assert.Equal(-69.8571, ruta.OrigenLongitud);
        Assert.Equal(18.4700, ruta.DestinoLatitud);
        Assert.Equal(-69.8900, ruta.DestinoLongitud);
        Assert.Equal("Ana Perez", ruta.ContactoNombre);
        Assert.Equal("809-555-1234", ruta.ContactoTelefono);
    }

    // RF-NEG-06
    [Fact]
    public async Task VerRutaAsync_EnvioSinAsignar_SeRechaza()
    {
        // Arrange
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();
        var servicioPermisos = new ServicioPermisos(coreDb, reloj);
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj);

        var creado = await servicio.CrearAsync(5, DatosValidos());

        // Act
        var resultado = await servicio.VerRutaAsync(10, creado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeSinRuta, resultado.Error);
    }
}
