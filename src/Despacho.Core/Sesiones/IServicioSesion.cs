using Despacho.Core.Comun;

namespace Despacho.Core.Sesiones;

public interface IServicioSesion
{
    Task<Resultado<string>> IniciarAsync(string? correo, string? contrasena);
    Task<UsuarioActual?> ObtenerUsuarioAsync(string? token);
    Task CerrarAsync(string? token);
}
