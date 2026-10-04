namespace Despacho.Core.Correos;

public class CorreoEnCola
{
    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public DateTime CreadoEnUtc { get; set; }
    public bool Enviado { get; set; } = false;
    public DateTime? EnviadoEnUtc { get; set; }
}
