using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Correos;

public sealed class ProcesadorCola : IProcesadorCola
{
    public const string ErrorServidor = "No se pudo comunicar con el servidor de correo.";
    public const string ErrorDestinatario = "El servidor de correo rechazó al destinatario.";
    public const string ErrorTiempo = "El servidor de correo tardó demasiado en responder.";
    public const string ErrorGeneral = "No se pudo enviar el correo.";

    private readonly CoreDbContext _db;
    private readonly IRemitenteCorreo _remitente;
    private readonly IReloj _reloj;

    public ProcesadorCola(CoreDbContext db, IRemitenteCorreo remitente, IReloj reloj)
    {
        _db = db;
        _remitente = remitente;
        _reloj = reloj;
    }

    public async Task<ResumenEnvio> ProcesarPendientesAsync()
    {
        var pendientes = await _db.CorreosEnCola
            .Where(c => c.Estado == EstadoCorreo.Pendiente)
            .OrderBy(c => c.Id)
            .ToListAsync();

        int enviados = 0, reintentos = 0, fallidos = 0;

        foreach (var correo in pendientes)
        {
            try
            {
                await _remitente.EnviarAsync(correo.Destinatario, correo.Asunto, correo.Cuerpo);
                correo.MarcarEnviado(_reloj.AhoraUtc);
                enviados++;
            }
            catch (Exception ex)
            {
                correo.RegistrarFallo(_reloj.AhoraUtc, DescribirError(ex));
                if (correo.Estado == EstadoCorreo.Fallido)
                    fallidos++;
                else
                    reintentos++;
            }

            await _db.SaveChangesAsync();
        }

        return new ResumenEnvio(enviados, reintentos, fallidos);
    }

    private static string DescribirError(Exception ex)
    {
        return ex switch
        {
            System.Net.Mail.SmtpFailedRecipientException => ErrorDestinatario,
            System.Net.Mail.SmtpException => ErrorServidor,
            TimeoutException => ErrorTiempo,
            System.Net.Sockets.SocketException => ErrorServidor,
            _ => ErrorGeneral
        };
    }
}
