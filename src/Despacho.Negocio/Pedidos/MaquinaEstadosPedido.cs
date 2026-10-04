namespace Despacho.Negocio.Pedidos;

public static class MaquinaEstadosPedido
{
    private static readonly Dictionary<EstadoPedido, EstadoPedido[]> Transiciones = new()
    {
        { EstadoPedido.Pendiente, new[] { EstadoPedido.Asignado, EstadoPedido.Cancelado } },
        { EstadoPedido.Asignado, new[] { EstadoPedido.EnRuta, EstadoPedido.Cancelado } },
        { EstadoPedido.EnRuta, new[] { EstadoPedido.Entregado } },
        { EstadoPedido.Entregado, Array.Empty<EstadoPedido>() },
        { EstadoPedido.Cancelado, Array.Empty<EstadoPedido>() }
    };

    private static readonly (EstadoPedido Desde, EstadoPedido Hacia)[] Prohibidas =
    {
        (EstadoPedido.Entregado, EstadoPedido.EnRuta)
    };

    public static bool EsTerminal(EstadoPedido estado) => Transiciones[estado].Length == 0;

    public static bool PuedeCambiar(EstadoPedido desde, EstadoPedido hacia) =>
        !Prohibidas.Contains((desde, hacia)) && Transiciones[desde].Contains(hacia);
}
