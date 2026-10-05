using Despacho.Core.Comun;

namespace Despacho.Core.Administracion;

public interface IServicioAdministracion
{
    Task<IReadOnlyList<UsuarioListado>> ListarAsync();
    Task<Resultado> CambiarRolAsync(int usuarioId, string? rol);
    Task<Resultado> DesactivarAsync(int adminId, int usuarioId);
    Task<Resultado> ReactivarAsync(int usuarioId);
    Task PromoverAdministradorInicialAsync(string? correo);
}
