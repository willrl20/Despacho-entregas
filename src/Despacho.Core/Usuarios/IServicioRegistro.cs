using System.Threading.Tasks;
using Despacho.Core.Comun;

namespace Despacho.Core.Usuarios;

public interface IServicioRegistro
{
    Task<Resultado> RegistrarAsync(string? correo, string? contrasena);

    Task<Resultado> ActivarAsync(string? token);

    Task<Resultado> ReenviarActivacionAsync(string? correo);
}