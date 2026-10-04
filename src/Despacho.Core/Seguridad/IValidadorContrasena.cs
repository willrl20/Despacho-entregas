using Despacho.Core.Comun;

namespace Despacho.Core.Seguridad;

public interface IValidadorContrasena
{
    Resultado Validar(string? contrasena);
}