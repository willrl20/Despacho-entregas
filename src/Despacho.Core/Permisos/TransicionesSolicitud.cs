namespace Despacho.Core.Permisos;

public static class TransicionesSolicitud
{
    private static readonly Dictionary<EstadoSolicitud, EstadoSolicitud[]> Permitidas = new()
    {
        { EstadoSolicitud.Pendiente, new[] { EstadoSolicitud.Aprobada, EstadoSolicitud.Rechazada } },
        { EstadoSolicitud.Aprobada, new[] { EstadoSolicitud.Aplicada } },
        { EstadoSolicitud.Aplicada, Array.Empty<EstadoSolicitud>() },
        { EstadoSolicitud.Rechazada, new[] { EstadoSolicitud.Aplicada } }
    };

    public static bool EsPermitida(EstadoSolicitud desde, EstadoSolicitud hacia) => Permitidas[desde].Contains(hacia);
}
