using Despacho.Core.Comun;

namespace Despacho.Core.Seguridad;

public sealed class ValidadorContrasena : IValidadorContrasena
{
    private const int LongitudMinima = 8;
    private const string MensajeError = "La contraseña debe tener al menos 8 caracteres e incluir letras y números.";

    public Resultado Validar(string? contrasena)
    {
        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < LongitudMinima)
        {
            return Resultado.Falla(MensajeError);
        }

        if (!contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
        {
            return Resultado.Falla(MensajeError);
        }

        return Resultado.Ok();
    }
}