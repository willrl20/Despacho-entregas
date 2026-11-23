using Despacho.Core.Comun;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Notificaciones;

public sealed class ServicioNotificaciones : IServicioNotificaciones
{
    public const int LargoMaximoTitulo = 200;
    public const int LargoMaximoMensaje = 1000;

    public const string MensajeDatosInvalidos = "El título y el mensaje de la notificación son obligatorios y no pueden ser tan largos.";
    public const string MensajeUsuarioNoExiste = "El usuario no existe.";
    public const string MensajeNoExiste = "La notificación no existe.";

    private readonly CoreDbContext _db;
    private readonly IColaCorreos _cola;
    private readonly IReloj _reloj;

    public ServicioNotificaciones(CoreDbContext db, IColaCorreos cola, IReloj reloj)
    {
        _db = db;
        _cola = cola;
        _reloj = reloj;
    }

    public async Task<Resultado> NotificarAsync(int usuarioId, string titulo, string mensaje)
    {
        if (!DatosValidos(titulo, mensaje))
            return Resultado.Falla(MensajeDatosInvalidos);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
            return Resultado.Falla(MensajeUsuarioNoExiste);

        var correoActivo = !await _db.PreferenciasNotificacion
            .AnyAsync(p => p.UsuarioId == usuarioId && !p.CorreoActivo);

        Agregar(usuario, titulo, mensaje, correoActivo);
        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado> NotificarAdministradoresAsync(string titulo, string mensaje)
    {
        if (!DatosValidos(titulo, mensaje))
            return Resultado.Falla(MensajeDatosInvalidos);

        var administradores = await _db.Usuarios
            .Where(u => u.Rol == Rol.Administrador && u.Activada && !u.Desactivado)
            .ToListAsync();

        if (administradores.Count == 0)
            return Resultado.Ok();

        var ids = administradores.Select(u => u.Id).ToList();
        var sinCorreo = await _db.PreferenciasNotificacion
            .Where(p => ids.Contains(p.UsuarioId) && !p.CorreoActivo)
            .Select(p => p.UsuarioId)
            .ToListAsync();

        foreach (var administrador in administradores)
            Agregar(administrador, titulo, mensaje, !sinCorreo.Contains(administrador.Id));

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<NotificacionResumen>> ListarAsync(int usuarioId, bool soloNoLeidas)
    {
        var consulta = _db.Notificaciones.Where(n => n.DestinatarioId == usuarioId);

        if (soloNoLeidas)
            consulta = consulta.Where(n => !n.Leida);

        return await consulta
            .OrderByDescending(n => n.CreadaEnUtc)
            .ThenByDescending(n => n.Id)
            .Select(n => new NotificacionResumen(n.Id, n.Titulo, n.Mensaje, n.Leida, n.CreadaEnUtc))
            .ToListAsync();
    }

    public async Task<Resultado> MarcarLeidaAsync(int usuarioId, int notificacionId)
    {
        var notificacion = await _db.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == notificacionId && n.DestinatarioId == usuarioId);

        if (notificacion == null)
            return Resultado.Falla(MensajeNoExiste);

        notificacion.Leida = true;
        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado> CambiarPreferenciaCorreoAsync(int usuarioId, bool correoActivo)
    {
        if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId))
            return Resultado.Falla(MensajeUsuarioNoExiste);

        var preferencia = await _db.PreferenciasNotificacion.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
        if (preferencia == null)
            _db.PreferenciasNotificacion.Add(new PreferenciaNotificacion { UsuarioId = usuarioId, CorreoActivo = correoActivo });
        else
            preferencia.CorreoActivo = correoActivo;

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    private static bool DatosValidos(string? titulo, string? mensaje)
    {
        return !string.IsNullOrWhiteSpace(titulo) && titulo.Length <= LargoMaximoTitulo
            && !string.IsNullOrWhiteSpace(mensaje) && mensaje.Length <= LargoMaximoMensaje;
    }

    private void Agregar(Usuario destinatario, string titulo, string mensaje, bool correoActivo)
    {
        _db.Notificaciones.Add(new Notificacion
        {
            DestinatarioId = destinatario.Id,
            Titulo = titulo,
            Mensaje = mensaje,
            CreadaEnUtc = _reloj.AhoraUtc
        });

        if (correoActivo)
            _cola.Encolar(destinatario.Correo, titulo, mensaje);
    }
}
