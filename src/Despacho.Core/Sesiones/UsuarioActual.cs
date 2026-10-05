using Despacho.Core.Usuarios;

namespace Despacho.Core.Sesiones;

public sealed record UsuarioActual(int Id, string Correo, Rol Rol);
