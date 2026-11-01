using Despacho.Negocio.Pedidos;

namespace Despacho.Negocio.Tests;

public class MaquinaEstadosPedidoTests
{
    private static Pedido CrearPedidoEn(EstadoPedido estado)
    {
        var pedido = new Pedido();

        switch (estado)
        {
            case EstadoPedido.Pendiente:
                break;
            case EstadoPedido.Asignado:
                pedido.CambiarEstado(EstadoPedido.Asignado);
                break;
            case EstadoPedido.EnRuta:
                pedido.CambiarEstado(EstadoPedido.Asignado);
                pedido.CambiarEstado(EstadoPedido.EnRuta);
                break;
            case EstadoPedido.Entregado:
                pedido.CambiarEstado(EstadoPedido.Asignado);
                pedido.CambiarEstado(EstadoPedido.EnRuta);
                pedido.CambiarEstado(EstadoPedido.Entregado);
                break;
            case EstadoPedido.Cancelado:
                pedido.CambiarEstado(EstadoPedido.Cancelado);
                break;
        }

        return pedido;
    }

    // RF-NEG-03
    [Theory]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.Cancelado)]
    [InlineData(EstadoPedido.Asignado, EstadoPedido.Cancelado)]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.Asignado)]
    [InlineData(EstadoPedido.Asignado, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.EnRuta, EstadoPedido.Entregado)]
    public void CambiarEstado_TransicionPermitida_CambiaElEstado(EstadoPedido desde, EstadoPedido hacia)
    {
        // Arrange
        var pedido = CrearPedidoEn(desde);

        // Act
        var resultado = pedido.CambiarEstado(hacia);

        // Assert
        Assert.True(resultado);
        Assert.Equal(hacia, pedido.Estado);
    }

    // RF-NEG-04
    [Theory]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.Entregado, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.Asignado, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.EnRuta, EstadoPedido.Cancelado)]
    [InlineData(EstadoPedido.Cancelado, EstadoPedido.Pendiente)]
    public void CambiarEstado_TransicionProhibida_SeRechazaYElEstadoNoCambia(EstadoPedido desde, EstadoPedido hacia)
    {
        // Arrange
        var pedido = CrearPedidoEn(desde);

        // Act
        var resultado = pedido.CambiarEstado(hacia);

        // Assert
        Assert.False(resultado);
        Assert.Equal(desde, pedido.Estado);
    }

    // RF-NEG-05
    [Theory]
    [InlineData(EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.Cancelado)]
    public void EstadoTerminal_NingunaTransicionParteDeEl(EstadoPedido terminal)
    {
        // Arrange
        // Act
        // Assert
        Assert.True(MaquinaEstadosPedido.EsTerminal(terminal));

        foreach (var valor in Enum.GetValues<EstadoPedido>())
        {
            Assert.False(MaquinaEstadosPedido.PuedeCambiar(terminal, valor));
        }
    }
}
