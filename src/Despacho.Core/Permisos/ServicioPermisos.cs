using Despacho.Core.Auditoria;
using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

    public const string TituloSolicitudCreada = "Nueva solicitud de permiso";
    public const string TituloSolicitudAprobada = "Solicitud de permiso aprobada";
    public const string TituloSolicitudRechazada = "Solicitud de permiso rechazada";

    private readonly CoreDbContext _db;
    private readonly IReloj _reloj;
    private readonly IServicioNotificaciones _notificaciones;
    private readonly ILogger _logger;

    public ServicioPermisos(CoreDbContext db, IReloj reloj, IServicioNotificaciones notificaciones, ILogger<ServicioPermisos>? logger = null)
    {
        _db = db;
        _reloj = reloj;
        _notificaciones = notificaciones;
        _logger = logger ?? NullLogger<ServicioPermisos>.Instance;
    }

    public async Task<Resultado<int>> CrearAsync(int solicitanteId, string? permiso)
    {
        if (!Enum.TryParse<Permiso>(permiso, true, out var permisoPedido) || !Enum.IsDefined(permisoPedido))
            return Resultado<int>.Falla(MensajePermisoInvalido);

        var usuario = await _db.Usuarios.FindAsync(solicitanteId);
        if (usuario == null)
            return Resultado<int>.Falla(MensajePermisoInvalido);

        if (await TienePermisoAsync(solicitanteId, permisoPedido))
            return Resultado<int>.Falla(MensajeYaTienePermiso);

        var existePendiente = await _db.SolicitudesPermiso
            .AnyAsync(s => s.SolicitanteId == solicitanteId && s.PermisoSolicitado == permisoPedido && s.Estado == EstadoSolicitud.Pendiente);

        if (existePendiente)
            return Resultado<int>.Falla(MensajeYaPendiente);

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            PermisoSolicitado = permisoPedido,
            CreadaEnUtc = _reloj.AhoraUtc
        };

        _db.SolicitudesPermiso.Add(solicitud);
        await _db.SaveChangesAsync();

        await AvisoSeguro.EjecutarAsync(_logger, "SolicitudCreada", solicitanteId, () =>
            _notificaciones.NotificarAdministradoresAsync(
                TituloSolicitudCreada,
                $"El usuario {usuario.Correo} pidió el permiso {permisoPedido}."));

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

        if (solicitud.SolicitanteId == aprobadorId)
            return Resultado.Falla(MensajePropia);

        var estadoAnterior = solicitud.Estado.ToString();

        if (!solicitud.Aprobar(aprobadorId, _reloj.AhoraUtc))
            return Resultado.Falla(MensajeNoPendiente);

        var usuario = await _db.Usuarios.FindAsync(solicitud.SolicitanteId);
        if (usuario == null)
            return Resultado.Falla(MensajeNoExiste);

        AplicarPermiso(usuario, solicitud.PermisoSolicitado);
        solicitud.MarcarAplicada();

        _db.RegistrarAuditoria(_reloj, aprobadorId, AccionesAuditoria.AprobarSolicitud, EntidadesAuditoria.SolicitudPermiso, solicitud.Id, estadoAnterior, solicitud.Estado.ToString());

        await _db.SaveChangesAsync();

        await AvisoSeguro.EjecutarAsync(_logger, "SolicitudAprobada", solicitud.SolicitanteId, () =>
            _notificaciones.NotificarAsync(
                solicitud.SolicitanteId,
                TituloSolicitudAprobada,
                $"Tu solicitud del permiso {solicitud.PermisoSolicitado} fue aprobada."));

        return Resultado.Ok();
    }

    public async Task<Resultado> RechazarAsync(int aprobadorId, int solicitudId, string? motivo)
    {
        var solicitud = await _db.SolicitudesPermiso.FindAsync(solicitudId);
        if (solicitud == null)
            return Resultado.Falla(MensajeNoExiste);

        if (solicitud.SolicitanteId == aprobadorId)
            return Resultado.Falla(MensajePropia);

        if (string.IsNullOrWhiteSpace(motivo))
            return Resultado.Falla(MensajeSinMotivo);

        if (motivo.Trim().Length > 500)
            return Resultado.Falla(MensajeMotivoLargo);

        var estadoAnterior = solicitud.Estado.ToString();

        if (!solicitud.Rechazar(aprobadorId, motivo, _reloj.AhoraUtc))
            return Resultado.Falla(MensajeNoPendiente);

        _db.RegistrarAuditoria(_reloj, aprobadorId, AccionesAuditoria.RechazarSolicitud, EntidadesAuditoria.SolicitudPermiso, solicitud.Id, estadoAnterior, TextosAuditoria.PrefijoRechazada + solicitud.MotivoRechazo);

        await _db.SaveChangesAsync();

        await AvisoSeguro.EjecutarAsync(_logger, "SolicitudRechazada", solicitud.SolicitanteId, () =>
            _notificaciones.NotificarAsync(
                solicitud.SolicitanteId,
                TituloSolicitudRechazada,
                $"Tu solicitud del permiso {solicitud.PermisoSolicitado} fue rechazada. Motivo: {solicitud.MotivoRechazo}"));

        return Resultado.Ok();
    }

    public async Task<bool> TienePermisoAsync(int usuarioId, Permiso permiso)
    {
        if (permiso == Permiso.Administrador)
        {
            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            return usuario != null && usuario.Rol == Rol.Administrador;
        }

        return await _db.PermisosConcedidos.AnyAsync(p => p.UsuarioId == usuarioId && p.Permiso == permiso);
    }

    private void AplicarPermiso(Usuario usuario, Permiso permiso)
    {
        if (permiso == Permiso.Administrador)
        {
            usuario.Rol = Rol.Administrador;
        }
        else if (permiso == Permiso.Repartidor)
        {
            var existeEnBd = _db.PermisosConcedidos.Local.Any(p => p.UsuarioId == usuario.Id && p.Permiso == permiso)
                || _db.PermisosConcedidos.Any(p => p.UsuarioId == usuario.Id && p.Permiso == permiso);

            if (!existeEnBd)
            {
                _db.PermisosConcedidos.Add(new PermisoConcedido
                {
                    UsuarioId = usuario.Id,
                    Permiso = permiso,
                    ConcedidoEnUtc = _reloj.AhoraUtc
                });
            }
        }
    }
}
