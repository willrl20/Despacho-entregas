namespace Despacho.Negocio.Pedidos;

public class LineaPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
