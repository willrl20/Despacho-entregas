namespace Despacho.Core.Usuarios;

public class Usuario
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public bool Activada { get; set; } = false;
    public DateTime CreadoEnUtc { get; set; }
    public Rol Rol { get; set; } = Rol.Estandar;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }
    public bool Desactivado { get; set; }
}
