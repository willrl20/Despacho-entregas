using System.Net.Mail;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ColaCorreosTests
{
    private static CoreDbContext CrearBase()
    {
        return new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static async Task AgregarCorreos(CoreDbContext db, int cantidad)
    {
        for (var i = 1; i <= cantidad; i++)
        {
            db.CorreosEnCola.Add(new CorreoEnCola
            {
                Destinatario = $"usuario{i}@test.com",
                Asunto = $"Asunto {i}",
                Cuerpo = "Cuerpo",
                CreadoEnUtc = RelojFijo.Ahora.AddMinutes(i)
            });
        }

        await db.SaveChangesAsync();
    }

    // C-14 RF-NOT-08
    [Fact]
    public async Task CrearAsync_ElServidorDeCorreoNoSeUsa_RespondeExitoYElCorreoQuedaPendiente()
    {
        // Arrange
        using var db = CrearBase();
        var reloj = new RelojFijo();
        db.Usuarios.Add(new Usuario { Id = 1, Correo = "a1@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador });
        db.Usuarios.Add(new Usuario { Id = 4, Correo = "repartidor@test.com", HashContrasena = "hash", Activada = true });
        await db.SaveChangesAsync();
        var notificaciones = new ServicioNotificaciones(db, new ColaCorreos(db, reloj), reloj);
        var servicio = new ServicioPermisos(db, reloj, notificaciones);

        // Act
        var resultado = await servicio.CrearAsync(4, "Repartidor");

        // Assert
        Assert.True(resultado.Exito);
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
        Assert.Equal(0, correo.Intentos);
    }

    // C-15 RF-NOT-09
    [Fact]
    public async Task ProcesarPendientesAsync_CorreosPendientes_QuedanEnviadoConSuFecha()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 2);
        var remitente = new RemitenteDoble();
        var procesador = new ProcesadorCola(db, remitente, new RelojFijo());

        // Act
        var resumen = await procesador.ProcesarPendientesAsync();

        // Assert
        Assert.Equal(2, resumen.Enviados);
        Assert.Equal(2, remitente.Llamadas);
        var correos = await db.CorreosEnCola.ToListAsync();
        Assert.All(correos, c =>
        {
            Assert.Equal(EstadoCorreo.Enviado, c.Estado);
            Assert.Equal(RelojFijo.Ahora, c.EnviadoEnUtc);
        });
    }

    // C-16 RF-NOT-10
    [Fact]
    public async Task ProcesarPendientesAsync_ServidorCaido_CadaVueltaSumaUnIntentoYSigueRecibiendoReintentos()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        var procesador = new ProcesadorCola(db, new RemitenteDoble(new SmtpException("no responde")), new RelojFijo());

        // Act
        await procesador.ProcesarPendientesAsync();
        await procesador.ProcesarPendientesAsync();

        // Assert
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(2, correo.Intentos);
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
    }

    // C-16 RF-NOT-10
    [Fact]
    public async Task ProcesarPendientesAsync_TresIntentosFallidos_QuedaFallidoConElUltimoError()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        var procesador = new ProcesadorCola(db, new RemitenteDoble(new SmtpException("no responde")), new RelojFijo());

        // Act
        for (var i = 0; i < CorreoEnCola.MaximoIntentos; i++)
            await procesador.ProcesarPendientesAsync();

        // Assert
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(CorreoEnCola.MaximoIntentos, correo.Intentos);
        Assert.Equal(EstadoCorreo.Fallido, correo.Estado);
        Assert.Equal(ProcesadorCola.ErrorServidor, correo.UltimoError);
        Assert.Equal(RelojFijo.Ahora, correo.UltimoIntentoEnUtc);
    }

    // C-16 RF-NOT-10
    [Fact]
    public async Task ProcesarPendientesAsync_CorreoFallido_NoSeVuelveAIntentarNiSeBorra()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        var remitente = new RemitenteDoble(new SmtpException("no responde"));
        var procesador = new ProcesadorCola(db, remitente, new RelojFijo());
        for (var i = 0; i < CorreoEnCola.MaximoIntentos; i++)
            await procesador.ProcesarPendientesAsync();

        // Act
        await procesador.ProcesarPendientesAsync();

        // Assert
        Assert.Equal(CorreoEnCola.MaximoIntentos, remitente.Llamadas);
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(CorreoEnCola.MaximoIntentos, correo.Intentos);
        Assert.Equal(EstadoCorreo.Fallido, correo.Estado);
    }

    // C-17 RF-NOT-11
    [Fact]
    public async Task ProcesarPendientesAsync_ErrorLargoConDatosSensibles_GuardaUnMensajeCortoSinEllos()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        var texto = "clave-secreta-123 C:\\app\\secretos\\smtp.cs en Despacho.Core.Correos.ProcesadorCola " + new string('x', 2000);
        var procesador = new ProcesadorCola(db, new RemitenteDoble(new InvalidOperationException(texto)), new RelojFijo());

        // Act
        await procesador.ProcesarPendientesAsync();

        // Assert
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.False(string.IsNullOrWhiteSpace(correo.UltimoError));
        Assert.True(correo.UltimoError!.Length <= 500);
        Assert.DoesNotContain("clave-secreta-123", correo.UltimoError);
        Assert.DoesNotContain("C:\\", correo.UltimoError);
    }

    // C-17 RF-NOT-11
    [Fact]
    public async Task ListarAsync_CorreoEnviado_MuestraSusDatosDeHistoria()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        await new ProcesadorCola(db, new RemitenteDoble(), new RelojFijo()).ProcesarPendientesAsync();
        var servicio = new ServicioCorreos(db);

        // Act
        var resultado = await servicio.ListarAsync(null);

        // Assert
        Assert.True(resultado.Exito);
        var correo = Assert.Single(resultado.Valor!);
        Assert.Equal("usuario1@test.com", correo.Destinatario);
        Assert.Equal("Asunto 1", correo.Asunto);
        Assert.Equal("Enviado", correo.Estado);
        Assert.Equal(1, correo.Intentos);
        Assert.Equal(RelojFijo.Ahora.AddMinutes(1), correo.CreadoEnUtc);
        Assert.Equal(RelojFijo.Ahora, correo.UltimoIntentoEnUtc);
        Assert.Equal(RelojFijo.Ahora, correo.EnviadoEnUtc);
        Assert.Null(correo.UltimoError);
    }

    // C-17 RF-NOT-11
    [Fact]
    public async Task ListarAsync_ConFiltroDeEstado_DevuelveSoloEseEstadoDelMasNuevoAlMasViejo()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 3);
        var primero = await db.CorreosEnCola.SingleAsync(c => c.Asunto == "Asunto 1");
        primero.MarcarEnviado(RelojFijo.Ahora);
        await db.SaveChangesAsync();
        var servicio = new ServicioCorreos(db);

        // Act
        var resultado = await servicio.ListarAsync("pendiente");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(new[] { "Asunto 3", "Asunto 2" }, resultado.Valor!.Select(c => c.Asunto).ToArray());
    }

    // C-17 RF-NOT-11, RD-07
    [Fact]
    public async Task ListarAsync_EstadoInvalido_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioCorreos(db);

        // Act
        var resultado = await servicio.ListarAsync("Roto");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioCorreos.MensajeEstadoInvalido, resultado.Error);
    }

    // C-19 RF-NOT-12
    [Fact]
    public async Task ProcesarPendientesAsync_DosVecesSeguidas_NoReenviaNiCambiaLosIntentos()
    {
        // Arrange
        using var db = CrearBase();
        await AgregarCorreos(db, 1);
        var remitente = new RemitenteDoble();
        var procesador = new ProcesadorCola(db, remitente, new RelojFijo());
        await procesador.ProcesarPendientesAsync();

        // Act
        var resumen = await procesador.ProcesarPendientesAsync();

        // Assert
        Assert.Equal(0, resumen.Enviados);
        Assert.Equal(1, remitente.Llamadas);
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(1, correo.Intentos);
        Assert.Equal(EstadoCorreo.Enviado, correo.Estado);
    }
}
