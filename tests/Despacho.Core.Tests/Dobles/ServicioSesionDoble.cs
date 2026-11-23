using Despacho.Core.Comun;
using Despacho.Core.Sesiones;

namespace Despacho.Core.Tests.Dobles;

public sealed class ServicioSesionDoble : IServicioSesion
{
    private readonly UsuarioActual? _usuario;

    public ServicioSesionDoble(UsuarioActual? usuario)
    {
        _usuario = usuario;
    }

    public Task<Resultado<string>> IniciarAsync(string? correo, string? contrasena) =>
        Task.FromResult(Resultado<string>.Falla("No usado."));

    public Task<UsuarioActual?> ObtenerUsuarioAsync(string? token) => Task.FromResult(_usuario);

    public Task CerrarAsync(string? token) => Task.CompletedTask;
}
