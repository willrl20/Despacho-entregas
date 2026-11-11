using Despacho.Core.Comun;

namespace Despacho.Negocio.Direcciones;

public interface IGeocodificador
{
    Task<Resultado<string>> ObtenerDireccionAsync(double latitud, double longitud);
}
