using Despacho.Core.Comun;

namespace Despacho.Negocio.Direcciones;

public interface IServicioDirecciones
{
    Task<Resultado<DireccionesEnvio>> VerDireccionesAsync(int repartidorId, int envioId);
}
