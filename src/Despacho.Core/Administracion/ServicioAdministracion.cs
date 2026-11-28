using Despacho.Core.Auditoria;
using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Administracion;

public sealed class ServicioAdministracion : IServicioAdministracion
{
    private readonly CoreDbContext _db;
    private readonly IReloj _reloj;

    private const string MensajeNoExiste = "El usuario no existe.";
    private const string MensajeRolInvalido = "El rol debe ser Administrador o Estandar.";
    private const string MensajeAutoDesactivar = "No puedes desactivar tu propia cuenta.";

    public ServicioAdministracion(CoreDbContext db, IReloj? reloj = null)
    {
        _db = db;
        _reloj = reloj ?? new RelojSistema();
    }

    public async Task<IReadOnlyList<UsuarioListado>> ListarAsync()
    {
        return await _db.Usuarios
            .OrderBy(u => u.Id)
            .Select(u => new UsuarioListado(u.Id, u.Correo, u.Rol.ToString(), u.Activada, u.Desactivado))
            .ToListAsync();
    }

    public async Task<Resultado> CambiarRolAsync(int adminId, int usuarioId, string? rol)
    {
        if (!Enum.TryParse<Rol>(rol, true, out var nuevoRol) || !Enum.IsDefined(nuevoRol))
            return Resultado.Falla(MensajeRolInvalido);

        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        var rolAnterior = usuario.Rol.ToString();
        usuario.Rol = nuevoRol;
        _db.RegistrarAuditoria(_reloj, adminId, AccionesAuditoria.CambiarRol, EntidadesAuditoria.Usuario, usuario.Id, rolAnterior, nuevoRol.ToString());
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

        var estadoAnterior = usuario.Desactivado ? TextosAuditoria.Desactivado : TextosAuditoria.Activo;
        usuario.Desactivado = true;
        _db.RegistrarAuditoria(_reloj, adminId, AccionesAuditoria.DesactivarUsuario, EntidadesAuditoria.Usuario, usuario.Id, estadoAnterior, TextosAuditoria.Desactivado);

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

    public async Task<Resultado> ReactivarAsync(int adminId, int usuarioId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        var estadoAnteriorReactivar = usuario.Desactivado ? TextosAuditoria.Desactivado : TextosAuditoria.Activo;
        usuario.Desactivado = false;
        _db.RegistrarAuditoria(_reloj, adminId, AccionesAuditoria.ReactivarUsuario, EntidadesAuditoria.Usuario, usuario.Id, estadoAnteriorReactivar, TextosAuditoria.Activo);
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
