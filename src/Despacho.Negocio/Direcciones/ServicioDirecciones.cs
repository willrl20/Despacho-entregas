using Despacho.Core.Comun;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Direcciones;

public sealed class ServicioDirecciones : IServicioDirecciones
{
    private readonly NegocioDbContext _db;
    private readonly IGeocodificador _geocodificador;

    public const string MensajeNoExiste = "El pedido no existe.";
    public const string MensajeSinDestino = "No puedes ver el destino de este pedido.";

    public ServicioDirecciones(NegocioDbContext db, IGeocodificador geocodificador)
    {
        _db = db;
        _geocodificador = geocodificador;
    }

    public async Task<Resultado<DireccionDestino>> VerDireccionDestinoAsync(int repartidorId, int pedidoId)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.Id == pedidoId);
        if (pedido == null)
            return Resultado<DireccionDestino>.Falla(MensajeNoExiste);

        if (pedido.RepartidorId != repartidorId
            || (pedido.Estado != EstadoPedido.Asignado && pedido.Estado != EstadoPedido.EnRuta))
            return Resultado<DireccionDestino>.Falla(MensajeSinDestino);

        var direccion = await _geocodificador.ObtenerDireccionAsync(pedido.DestinoLatitud, pedido.DestinoLongitud);
        if (!direccion.Exito)
            return Resultado<DireccionDestino>.Falla(direccion.Error!);

        return Resultado<DireccionDestino>.Ok(new DireccionDestino(pedido.DestinoLatitud, pedido.DestinoLongitud, pedido.DestinoReferencia, direccion.Valor!));
    }
}
