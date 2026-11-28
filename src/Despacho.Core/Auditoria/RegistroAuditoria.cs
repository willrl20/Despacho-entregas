using System.ComponentModel.DataAnnotations.Schema;

namespace Despacho.Core.Auditoria;

public class RegistroAuditoria
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public DateTime FechaUtc { get; set; }
    public string ValorAnterior { get; set; } = string.Empty;
    public string ValorNuevo { get; set; } = string.Empty;

    [NotMapped]
    public object? EntidadReferencia { get; set; }
}
