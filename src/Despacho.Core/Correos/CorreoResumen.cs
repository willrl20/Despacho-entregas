namespace Despacho.Core.Correos;

public sealed record CorreoResumen(
    int Id,
    string Destinatario,
    string Asunto,
    string Estado,
    int Intentos,
    DateTime CreadoEnUtc,
    DateTime? UltimoIntentoEnUtc,
    DateTime? EnviadoEnUtc,
    string? UltimoError);
