using Despacho.Core.Comun;
using Microsoft.Extensions.Logging;

namespace Despacho.Core.Notificaciones;

public static class AvisoSeguro
{
    public static async Task EjecutarAsync(ILogger logger, string evento, int usuarioId, Func<Task<Resultado>> notificar)
    {
        try
        {
            var resultado = await notificar();
            if (!resultado.Exito)
            {
                logger.LogWarning(
                    "No se pudo crear la notificación del evento {Evento} para el usuario {UsuarioId}. Motivo: {Motivo}",
                    evento,
                    usuarioId,
                    resultado.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "Falló la notificación del evento {Evento} para el usuario {UsuarioId}. Tipo de error: {TipoError}",
                evento,
                usuarioId,
                ex.GetType().Name);
        }
    }
}
