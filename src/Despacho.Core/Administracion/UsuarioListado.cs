namespace Despacho.Core.Administracion;

public sealed record UsuarioListado(int Id, string Correo, string Rol, bool Activada, bool Desactivado);
