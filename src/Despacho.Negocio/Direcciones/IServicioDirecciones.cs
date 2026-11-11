using Despacho.Core.Comun;

namespace Despacho.Negocio.Direcciones;

public interface IServicioDirecciones
{
    Task<Resultado<DireccionDestino>> VerDireccionDestinoAsync(int repartidorId, int pedidoId);
}
