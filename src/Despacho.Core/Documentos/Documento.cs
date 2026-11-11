namespace Despacho.Core.Documentos;

public class Documento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime SubidoEnUtc { get; set; }
    public int PropietarioId { get; set; }
    public string NombreAlmacenado { get; set; } = string.Empty;
    public bool Eliminado { get; set; }
    public DateTime? EliminadoEnUtc { get; set; }
}
