using Despacho.Core.Seguridad;

namespace Despacho.Core.Tests;

public class ValidadorContrasenaTests
{
    [Fact]
    public void Validar_ContrasenaConLetrasYNumeros_LaAcepta()
    {
        // Arrange (preparar)
        var validador = new ValidadorContrasena();

        // Act (actuar)
        var resultado = validador.Validar("Prueba123");

        // Assert (verificar)
        Assert.True(resultado.Exito);
    }

    [Fact]
    public void Validar_ContrasenaDeOchoCaracteresExactos_LaAcepta()
    {
        // Arrange
        var validador = new ValidadorContrasena();

        // Act
        var resultado = validador.Validar("Abcdef12");

        // Assert
        Assert.True(resultado.Exito);
    }

    [Fact]
    public void Validar_ContrasenaSinNumeros_LaRechaza()
    {
        // Arrange
        var validador = new ValidadorContrasena();

        // Act
        var resultado = validador.Validar("solotexto");

        // Assert
        Assert.False(resultado.Exito);
    }
}