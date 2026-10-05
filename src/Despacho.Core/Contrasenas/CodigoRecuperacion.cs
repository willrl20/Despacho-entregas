namespace Despacho.Core.Contrasenas;

public class CodigoRecuperacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime VenceEnUtc { get; set; }
    public bool Usado { get; set; } = false;
    public bool Invalidado { get; set; } = false;
}
