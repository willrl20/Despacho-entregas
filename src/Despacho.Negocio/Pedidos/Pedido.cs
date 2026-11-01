namespace Despacho.Negocio.Pedidos;

public class Pedido
{
    public int Id { get; set; }
    public int TiendaId { get; set; }
    public int DestinatarioId { get; set; }
    public int? RepartidorId { get; set; }
    public EstadoPedido Estado { get; private set; } = EstadoPedido.Pendiente;
    public DateTime CreadoEnUtc { get; set; }
    public double DestinoLatitud { get; set; }
    public double DestinoLongitud { get; set; }
    public string? DestinoReferencia { get; set; }
    public List<LineaPedido> Lineas { get; set; } = new();

    public bool CambiarEstado(EstadoPedido nuevo)
    {
        if (!MaquinaEstadosPedido.PuedeCambiar(Estado, nuevo))
            return false;

        Estado = nuevo;
        return true;
    }
}
