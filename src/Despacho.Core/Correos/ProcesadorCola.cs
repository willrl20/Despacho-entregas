using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Correos;

public sealed class ProcesadorCola : IProcesadorCola
{
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
        var pendientes = await _db.CorreosEnCola.Where(c => !c.Enviado).OrderBy(c => c.Id).ToListAsync();
        int enviados = 0, fallidos = 0;

        foreach (var correo in pendientes)
        {
            try
            {
                await _remitente.EnviarAsync(correo.Destinatario, correo.Asunto, correo.Cuerpo);
                correo.Enviado = true;
                correo.EnviadoEnUtc = _reloj.AhoraUtc;
                await _db.SaveChangesAsync();
                enviados++;
            }
            catch (Exception)
            {
                fallidos++;
            }
        }

        return new ResumenEnvio(enviados, fallidos);
    }
}
