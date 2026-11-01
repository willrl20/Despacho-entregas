using Despacho.Core.Comun;

namespace Despacho.Negocio.Pedidos;

public interface IServicioPedidos
{
    Task<Resultado<int>> RegistrarAsync(int clienteId, int tiendaId, double latitud, double longitud, string? referencia, IReadOnlyList<LineaNueva>? lineas);
    Task<Resultado<IReadOnlyList<PedidoResumen>>> ListarDisponiblesAsync(int repartidorId);
    Task<Resultado> AceptarAsync(int repartidorId, int pedidoId);
    Task<Resultado<DestinoPedido>> VerDestinoAsync(int repartidorId, int pedidoId);
    Task<Resultado> MarcarEnRutaAsync(int repartidorId, int pedidoId);
    Task<Resultado> ConfirmarEntregaAsync(int repartidorId, int pedidoId);
    Task<Resultado> CancelarAsync(int usuarioId, bool esAdministrador, int pedidoId);
    Task<IReadOnlyList<PedidoResumen>> ListarDeClienteAsync(int clienteId);
    Task<IReadOnlyList<PedidoResumen>> ListarTodosAsync();
}
