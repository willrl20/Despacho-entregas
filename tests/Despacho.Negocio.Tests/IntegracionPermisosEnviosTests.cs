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

public class IntegracionPermisosEnviosTests
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

    // Integración RF-NEG-02 con Gestión de permisos
    [Fact]
    public async Task AceptarAsync_ConSolicitudSinAprobar_SeRechazaYElEnvioSiguePendiente()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();

        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 20, Correo = "admin@test.com", Rol = Rol.Administrador, Activada = true });
        await coreDb.SaveChangesAsync();

        var servicioPermisos = new ServicioPermisos(coreDb, reloj, new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj));
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj, new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj));

        var creado = await servicio.CrearAsync(30, DatosValidos());

        await servicioPermisos.CrearAsync(10, "Repartidor");

        // Act
        var resultado = await servicio.AceptarAsync(10, creado.Valor);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioEnvios.MensajeSinPermiso, resultado.Error);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Pendiente, envio!.Estado);
    }

    // Integración RF-NEG-02 con Gestión de permisos
    [Fact]
    public async Task AceptarAsync_DespuesDeAprobarElPermiso_ElEnvioQuedaAsignado()
    {
        // Arrange
        var coreDb = new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var negocioDb = new NegocioDbContext(new DbContextOptionsBuilder<NegocioDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var reloj = new RelojFijo();

        coreDb.Usuarios.Add(new Usuario { Id = 10, Correo = "repartidor@test.com", Rol = Rol.Estandar, Activada = true });
        coreDb.Usuarios.Add(new Usuario { Id = 20, Correo = "admin@test.com", Rol = Rol.Administrador, Activada = true });
        await coreDb.SaveChangesAsync();

        var servicioPermisos = new ServicioPermisos(coreDb, reloj, new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj));
        var servicio = new ServicioEnvios(negocioDb, servicioPermisos, reloj, new ServicioNotificaciones(coreDb, new ColaCorreos(coreDb, reloj), reloj));

        var creado = await servicio.CrearAsync(30, DatosValidos());

        var solicitud = await servicioPermisos.CrearAsync(10, "Repartidor");
        var aprobacion = await servicioPermisos.AprobarAsync(20, solicitud.Valor);
        Assert.True(aprobacion.Exito);

        // Act
        var resultado = await servicio.AceptarAsync(10, creado.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var envio = await negocioDb.Envios.FindAsync(creado.Valor);
        Assert.Equal(EstadoEnvio.Asignado, envio!.Estado);
        Assert.Equal(10, envio.RepartidorId);
    }
}
