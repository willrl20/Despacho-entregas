namespace Despacho.Core.Notificaciones;

public class PreferenciaNotificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public bool CorreoActivo { get; set; } = true;
}
