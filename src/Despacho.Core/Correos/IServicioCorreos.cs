using Despacho.Core.Comun;

namespace Despacho.Core.Correos;

public interface IServicioCorreos
{
    Task<Resultado<IReadOnlyList<CorreoResumen>>> ListarAsync(string? estado);
}
