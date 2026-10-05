using Despacho.Core.Comun;

namespace Despacho.Core.Contrasenas;

public interface IServicioContrasenas
{
    Task<Resultado> SolicitarRecuperacionAsync(string? correo);
    Task<Resultado> RestablecerAsync(string? correo, string? codigo, string? contrasenaNueva);
    Task<Resultado> ForzarRestablecimientoAsync(int usuarioId);
    Task<Resultado> CambiarAsync(int usuarioId, string? contrasenaActual, string? contrasenaNueva);
}
