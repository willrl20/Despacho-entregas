namespace Despacho.Core.Permisos;

public class PermisoConcedido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Permiso Permiso { get; set; }
    public DateTime ConcedidoEnUtc { get; set; }
}
