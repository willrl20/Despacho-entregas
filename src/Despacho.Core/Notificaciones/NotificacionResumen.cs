namespace Despacho.Core.Notificaciones;

public sealed record NotificacionResumen(int Id, string Titulo, string Mensaje, bool Leida, DateTime CreadaEnUtc);
