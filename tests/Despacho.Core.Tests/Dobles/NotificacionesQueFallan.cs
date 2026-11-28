using Despacho.Core.Comun;
using Despacho.Core.Notificaciones;

namespace Despacho.Core.Tests.Dobles;

public sealed class NotificacionesQueFallan : IServicioNotificaciones
{
    public Task<Resultado> NotificarAsync(int usuarioId, string titulo, string mensaje) =>
        throw new InvalidOperationException("La base no responde.");

    public Task<Resultado> NotificarAdministradoresAsync(string titulo, string mensaje) =>
        throw new InvalidOperationException("La base no responde.");

    public Task<IReadOnlyList<NotificacionResumen>> ListarAsync(int usuarioId, bool soloNoLeidas) =>
        throw new InvalidOperationException("La base no responde.");

    public Task<Resultado> MarcarLeidaAsync(int usuarioId, int notificacionId) =>
        throw new InvalidOperationException("La base no responde.");

    public Task<Resultado> CambiarPreferenciaCorreoAsync(int usuarioId, bool correoActivo) =>
        throw new InvalidOperationException("La base no responde.");
}
