using Despacho.Negocio.Envios;

namespace Despacho.Negocio.Tests;

public class MaquinaEstadosEnvioTests
{
    private static Envio CrearEnvioEn(EstadoEnvio estado)
    {
        var envio = new Envio();

        switch (estado)
        {
            case EstadoEnvio.Pendiente:
                break;
            case EstadoEnvio.Asignado:
                envio.CambiarEstado(EstadoEnvio.Asignado);
                break;
            case EstadoEnvio.EnRuta:
                envio.CambiarEstado(EstadoEnvio.Asignado);
                envio.CambiarEstado(EstadoEnvio.EnRuta);
                break;
            case EstadoEnvio.Entregado:
                envio.CambiarEstado(EstadoEnvio.Asignado);
                envio.CambiarEstado(EstadoEnvio.EnRuta);
                envio.CambiarEstado(EstadoEnvio.Entregado);
                break;
            case EstadoEnvio.Cancelado:
                envio.CambiarEstado(EstadoEnvio.Cancelado);
                break;
        }

        return envio;
    }

    // RF-NEG-03
    [Theory]
    [InlineData(EstadoEnvio.Pendiente, EstadoEnvio.Cancelado)]
    [InlineData(EstadoEnvio.Asignado, EstadoEnvio.Cancelado)]
    [InlineData(EstadoEnvio.Pendiente, EstadoEnvio.Asignado)]
    [InlineData(EstadoEnvio.Asignado, EstadoEnvio.EnRuta)]
    [InlineData(EstadoEnvio.EnRuta, EstadoEnvio.Entregado)]
    public void CambiarEstado_TransicionPermitida_CambiaElEstado(EstadoEnvio desde, EstadoEnvio hacia)
    {
        // Arrange
        var envio = CrearEnvioEn(desde);

        // Act
        var resultado = envio.CambiarEstado(hacia);

        // Assert
        Assert.True(resultado);
        Assert.Equal(hacia, envio.Estado);
    }

    // RF-NEG-04
    [Theory]
    [InlineData(EstadoEnvio.Pendiente, EstadoEnvio.EnRuta)]
    [InlineData(EstadoEnvio.Entregado, EstadoEnvio.EnRuta)]
    [InlineData(EstadoEnvio.Pendiente, EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Asignado, EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.EnRuta, EstadoEnvio.Cancelado)]
    [InlineData(EstadoEnvio.Cancelado, EstadoEnvio.Pendiente)]
    public void CambiarEstado_TransicionProhibida_SeRechazaYElEstadoNoCambia(EstadoEnvio desde, EstadoEnvio hacia)
    {
        // Arrange
        var envio = CrearEnvioEn(desde);

        // Act
        var resultado = envio.CambiarEstado(hacia);

        // Assert
        Assert.False(resultado);
        Assert.Equal(desde, envio.Estado);
    }

    // RF-NEG-05
    [Theory]
    [InlineData(EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Cancelado)]
    public void EstadoTerminal_NingunaTransicionParteDeEl(EstadoEnvio terminal)
    {
        // Arrange
        // Act
        // Assert
        Assert.True(MaquinaEstadosEnvio.EsTerminal(terminal));

        foreach (var valor in Enum.GetValues<EstadoEnvio>())
        {
            Assert.False(MaquinaEstadosEnvio.PuedeCambiar(terminal, valor));
        }
    }
}
