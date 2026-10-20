using Despacho.Core.Usuarios;

namespace Despacho.Core.Permisos;

public class SolicitudPermiso
{
    public int Id { get; set; }
    public int SolicitanteId { get; set; }
    public Rol PermisoSolicitado { get; set; }
    public EstadoSolicitud Estado { get; private set; } = EstadoSolicitud.Pendiente;
    public int? AprobadorId { get; private set; }
    public DateTime CreadaEnUtc { get; set; }
    public DateTime? ResueltaEnUtc { get; private set; }
    public string? MotivoRechazo { get; private set; }

    public bool Aprobar(int aprobadorId, DateTime ahoraUtc)
    {
        if (!TransicionesSolicitud.EsPermitida(Estado, EstadoSolicitud.Aprobada))
            return false;

        Estado = EstadoSolicitud.Aprobada;
        AprobadorId = aprobadorId;
        ResueltaEnUtc = ahoraUtc;
        return true;
    }

    public bool MarcarAplicada()
    {
        if (!TransicionesSolicitud.EsPermitida(Estado, EstadoSolicitud.Aplicada))
            return false;

        Estado = EstadoSolicitud.Aplicada;
        return true;
    }

    public bool Rechazar(int aprobadorId, string? motivo, DateTime ahoraUtc)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return false;

        if (!TransicionesSolicitud.EsPermitida(Estado, EstadoSolicitud.Rechazada))
            return false;

        Estado = EstadoSolicitud.Rechazada;
        AprobadorId = aprobadorId;
        MotivoRechazo = motivo.Trim();
        ResueltaEnUtc = ahoraUtc;
        return true;
    }
}
