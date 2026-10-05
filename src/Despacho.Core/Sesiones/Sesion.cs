namespace Despacho.Core.Sesiones;

public class Sesion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreadaEnUtc { get; set; }
    public DateTime VenceEnUtc { get; set; }
    public bool Revocada { get; set; } = false;
}
