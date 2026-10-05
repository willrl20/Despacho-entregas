using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Administracion;

public sealed class ServicioAdministracion : IServicioAdministracion
{
    private readonly CoreDbContext _db;

    private const string MensajeNoExiste = "El usuario no existe.";
    private const string MensajeRolInvalido = "El rol debe ser Administrador o Estandar.";
    private const string MensajeAutoDesactivar = "No puedes desactivar tu propia cuenta.";

    public ServicioAdministracion(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UsuarioListado>> ListarAsync()
    {
        return await _db.Usuarios
            .OrderBy(u => u.Id)
            .Select(u => new UsuarioListado(u.Id, u.Correo, u.Rol.ToString(), u.Activada, u.Desactivado))
            .ToListAsync();
    }

    public async Task<Resultado> CambiarRolAsync(int usuarioId, string? rol)
    {
        if (!Enum.TryParse<Rol>(rol, true, out var nuevoRol) || !Enum.IsDefined(nuevoRol))
            return Resultado.Falla(MensajeRolInvalido);

        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        usuario.Rol = nuevoRol;
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> DesactivarAsync(int adminId, int usuarioId)
    {
        if (adminId == usuarioId)
            return Resultado.Falla(MensajeAutoDesactivar);

        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        usuario.Desactivado = true;

        var sesiones = await _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && !s.Revocada)
            .ToListAsync();

        foreach (var sesion in sesiones)
        {
            sesion.Revocada = true;
        }

        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> ReactivarAsync(int usuarioId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        usuario.Desactivado = false;
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task PromoverAdministradorInicialAsync(string? correo)
    {
        var c = NormalizadorCorreo.Normalizar(correo);
        if (c is null)
            return;

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == c);
        if (usuario is not null && usuario.Rol != Rol.Administrador)
        {
            usuario.Rol = Rol.Administrador;
            await _db.SaveChangesAsync();
        }
    }
}
