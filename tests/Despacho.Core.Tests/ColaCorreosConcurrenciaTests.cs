using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Tests.Dobles;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ColaCorreosConcurrenciaTests
{
    private sealed class RemitenteQueSimulaOtroProcesador : IRemitenteCorreo
    {
        private bool _yaSimulo;

        public ProcesadorCola? ProcesadorB { get; set; }
        public int Envios { get; private set; }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            if (!_yaSimulo)
            {
                _yaSimulo = true;
                await ProcesadorB!.ProcesarPendientesAsync();
            }

            Envios++;
        }
    }

    // Hallazgo 1 RF-NOT-12
    [Fact]
    public async Task ProcesarPendientesAsync_DosProcesadoresALaVez_EnvianCadaCorreoUnaSolaVez()
    {
        // Arrange
        var opciones = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var reloj = new RelojFijo();

        using (var siembra = new CoreDbContext(opciones))
        {
            siembra.CorreosEnCola.Add(new CorreoEnCola
            {
                Destinatario = "usuario@test.com",
                Asunto = "Asunto",
                Cuerpo = "Cuerpo",
                CreadoEnUtc = RelojFijo.Ahora
            });
            await siembra.SaveChangesAsync();
        }

        using var dbA = new CoreDbContext(opciones);
        using var dbB = new CoreDbContext(opciones);
        var remitente = new RemitenteQueSimulaOtroProcesador();
        var procesadorA = new ProcesadorCola(dbA, remitente, reloj);
        var procesadorB = new ProcesadorCola(dbB, remitente, reloj);
        remitente.ProcesadorB = procesadorB;

        // Act
        await procesadorA.ProcesarPendientesAsync();

        // Assert
        Assert.Equal(1, remitente.Envios);
    }
}
