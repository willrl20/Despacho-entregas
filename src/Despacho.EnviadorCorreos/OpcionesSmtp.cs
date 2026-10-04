namespace Despacho.EnviadorCorreos;

public sealed class OpcionesSmtp
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Remitente { get; set; } = string.Empty;
}
