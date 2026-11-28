namespace Despacho.Core.Auditoria;

public sealed record RegistroAuditoriaResumen(
    int Id,
    int UsuarioId,
    string Accion,
    string Entidad,
    int EntidadId,
    DateTime FechaUtc,
    string ValorAnterior,
    string ValorNuevo);
