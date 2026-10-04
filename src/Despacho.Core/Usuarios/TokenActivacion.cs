namespace Despacho.Core.Usuarios;

public class TokenActivacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime VenceEnUtc { get; set; }
    public bool Usado { get; set; } = false;
    public bool Invalidado { get; set; } = false;
}
