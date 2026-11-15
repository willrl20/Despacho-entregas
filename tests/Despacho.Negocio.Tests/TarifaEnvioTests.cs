using Despacho.Negocio.Envios;

namespace Despacho.Negocio.Tests;

public class TarifaEnvioTests
{
    // RF-NEG-02
    [Fact]
    public void CalcularCosto_HastaDosKmPequeno_CobraLaTarifaBase()
    {
        // Arrange
        var distanciaKm = 1.5;

        // Act
        var costo = TarifaEnvio.CalcularCosto(distanciaKm, TamanoPaquete.Pequeno);

        // Assert
        Assert.Equal(100m, costo);
    }

    // RF-NEG-02
    [Fact]
    public void CalcularCosto_CincoKmMediano_SumaKmExtraYMultiplica()
    {
        // Arrange
        var distanciaKm = 5;

        // Act
        var costo = TarifaEnvio.CalcularCosto(distanciaKm, TamanoPaquete.Mediano);

        // Assert
        Assert.Equal(200m, costo);
    }

    // RF-NEG-02
    [Fact]
    public void CalcularCosto_CincoKmGrande_AplicaMultiplicadorDeGrande()
    {
        // Arrange
        var distanciaKm = 5;

        // Act
        var costo = TarifaEnvio.CalcularCosto(distanciaKm, TamanoPaquete.Grande);

        // Assert
        Assert.Equal(240m, costo);
    }

    // RF-NEG-02
    [Fact]
    public void CalcularDistanciaKm_UnGradoDeLongitudEnElEcuador_DaUnos111Km()
    {
        // Arrange
        // Act
        var distancia = TarifaEnvio.CalcularDistanciaKm(0, 0, 0, 1);

        // Assert
        Assert.InRange(distancia, 111.1, 111.3);
    }
}
