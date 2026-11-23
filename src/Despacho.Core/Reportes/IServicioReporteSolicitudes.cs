using Despacho.Core.Comun;
using Despacho.Core.Sesiones;

namespace Despacho.Core.Reportes;

public interface IServicioReporteSolicitudes
{
    Task<Resultado<IReadOnlyList<FilaReporteSolicitudes>>> ObtenerAsync(UsuarioActual usuario, string? desde, string? hasta);
}
