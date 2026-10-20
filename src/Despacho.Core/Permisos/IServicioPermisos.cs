using Despacho.Core.Comun;

namespace Despacho.Core.Permisos;

public interface IServicioPermisos
{
    Task<Resultado<int>> CrearAsync(int solicitanteId, string? permiso);
    Task<IReadOnlyList<SolicitudResumen>> ListarPendientesAsync();
    Task<IReadOnlyList<SolicitudResumen>> ListarDeUsuarioAsync(int usuarioId);
    Task<Resultado> AprobarAsync(int aprobadorId, int solicitudId);
    Task<Resultado> RechazarAsync(int aprobadorId, int solicitudId, string? motivo);
}
