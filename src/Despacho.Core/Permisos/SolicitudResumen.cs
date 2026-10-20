namespace Despacho.Core.Permisos;

public sealed record SolicitudResumen(int Id, int SolicitanteId, string PermisoSolicitado, string Estado, int? AprobadorId, DateTime CreadaEnUtc, DateTime? ResueltaEnUtc, string? MotivoRechazo);
