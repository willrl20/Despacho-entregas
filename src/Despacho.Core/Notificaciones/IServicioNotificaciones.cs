using Despacho.Core.Comun;

namespace Despacho.Core.Notificaciones;

public interface IServicioNotificaciones
{
    Task<Resultado> NotificarAsync(int usuarioId, string titulo, string mensaje);

    Task<Resultado> NotificarAdministradoresAsync(string titulo, string mensaje);

    Task<IReadOnlyList<NotificacionResumen>> ListarAsync(int usuarioId, bool soloNoLeidas);

    Task<Resultado> MarcarLeidaAsync(int usuarioId, int notificacionId);

    Task<Resultado> CambiarPreferenciaCorreoAsync(int usuarioId, bool correoActivo);
}
