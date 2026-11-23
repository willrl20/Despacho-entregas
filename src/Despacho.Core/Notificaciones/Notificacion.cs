namespace Despacho.Core.Notificaciones;

public class Notificacion
{
    public int Id { get; set; }
    public int DestinatarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime CreadaEnUtc { get; set; }
}
