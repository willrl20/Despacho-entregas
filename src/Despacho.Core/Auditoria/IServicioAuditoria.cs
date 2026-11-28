using Despacho.Core.Comun;
using Despacho.Core.Sesiones;

namespace Despacho.Core.Auditoria;

public interface IServicioAuditoria
{
    Task<Resultado<IReadOnlyList<RegistroAuditoriaResumen>>> ConsultarAsync(UsuarioActual consultante, string? usuarioId, string? entidad, string? desde, string? hasta);
}
