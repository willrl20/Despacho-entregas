using Despacho.Core.Comun;
using Despacho.Core.Sesiones;

namespace Despacho.Negocio.Reportes;

public interface IServicioReporteRepartidores
{
    Task<Resultado<IReadOnlyList<FilaReporteRepartidores>>> ObtenerAsync(UsuarioActual usuario, string? desde, string? hasta);
}
