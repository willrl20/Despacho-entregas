using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Permisos;

public sealed class ServicioPermisos : IServicioPermisos
{
    private const string MensajePermisoInvalido = "El permiso solicitado no es válido.";
    private const string MensajeYaTienePermiso = "Ya tienes ese permiso.";
    private const string MensajeYaPendiente = "Ya tienes una solicitud pendiente de ese permiso.";
    private const string MensajeNoExiste = "La solicitud no existe.";
    private const string MensajePropia = "No puedes resolver tu propia solicitud.";
    private const string MensajeNoPendiente = "La solicitud no está pendiente.";
    private const string MensajeSinMotivo = "Escribe el motivo del rechazo.";
    private const string MensajeMotivoLargo = "El motivo no puede tener más de 500 caracteres.";

    private readonly CoreDbContext _db;
    private readonly IReloj _reloj;

    public ServicioPermisos(CoreDbContext db, IReloj reloj)
    {
        _db = db;
        _reloj = reloj;
    }

    public async Task<Resultado<int>> CrearAsync(int solicitanteId, string? permiso)
    {
        if (!Enum.TryParse<Rol>(permiso, true, out var rol) || !Enum.IsDefined(rol))
            return Resultado<int>.Falla(MensajePermisoInvalido);

        var usuario = await _db.Usuarios.FindAsync(solicitanteId);
        if (usuario == null)
            return Resultado<int>.Falla(MensajePermisoInvalido);

        if (usuario.Rol == rol)
            return Resultado<int>.Falla(MensajeYaTienePermiso);

        var existePendiente = await _db.SolicitudesPermiso
            .AnyAsync(s => s.SolicitanteId == solicitanteId && s.PermisoSolicitado == rol && s.Estado == EstadoSolicitud.Pendiente);

        if (existePendiente)
            return Resultado<int>.Falla(MensajeYaPendiente);

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            PermisoSolicitado = rol,
            CreadaEnUtc = _reloj.AhoraUtc
        };

        _db.SolicitudesPermiso.Add(solicitud);
        await _db.SaveChangesAsync();

        return Resultado<int>.Ok(solicitud.Id);
    }

    public async Task<IReadOnlyList<SolicitudResumen>> ListarPendientesAsync()
    {
        return await _db.SolicitudesPermiso
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderBy(s => s.CreadaEnUtc)
            .Select(s => new SolicitudResumen(s.Id, s.SolicitanteId, s.PermisoSolicitado.ToString(), s.Estado.ToString(), s.AprobadorId, s.CreadaEnUtc, s.ResueltaEnUtc, s.MotivoRechazo))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SolicitudResumen>> ListarDeUsuarioAsync(int usuarioId)
    {
        return await _db.SolicitudesPermiso
            .Where(s => s.SolicitanteId == usuarioId)
            .OrderByDescending(s => s.CreadaEnUtc)
            .Select(s => new SolicitudResumen(s.Id, s.SolicitanteId, s.PermisoSolicitado.ToString(), s.Estado.ToString(), s.AprobadorId, s.CreadaEnUtc, s.ResueltaEnUtc, s.MotivoRechazo))
            .ToListAsync();
    }

    public async Task<Resultado> AprobarAsync(int aprobadorId, int solicitudId)
    {
        var solicitud = await _db.SolicitudesPermiso.FindAsync(solicitudId);
        if (solicitud == null)
            return Resultado.Falla(MensajeNoExiste);

        if (!solicitud.Aprobar(aprobadorId, _reloj.AhoraUtc))
            return Resultado.Falla(MensajeNoPendiente);

        var usuario = await _db.Usuarios.FindAsync(solicitud.SolicitanteId);
        if (usuario == null)
            return Resultado.Falla(MensajeNoExiste);

        usuario.Rol = solicitud.PermisoSolicitado;
        solicitud.MarcarAplicada();

        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> RechazarAsync(int aprobadorId, int solicitudId, string? motivo)
    {
        var solicitud = await _db.SolicitudesPermiso.FindAsync(solicitudId);
        if (solicitud == null)
            return Resultado.Falla(MensajeNoExiste);

        if (string.IsNullOrWhiteSpace(motivo))
            return Resultado.Falla(MensajeSinMotivo);

        if (motivo.Trim().Length > 500)
            return Resultado.Falla(MensajeMotivoLargo);

        if (!solicitud.Rechazar(aprobadorId, motivo, _reloj.AhoraUtc))
            return Resultado.Falla(MensajeNoPendiente);

        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }
}
