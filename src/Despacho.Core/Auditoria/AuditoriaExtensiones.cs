using Despacho.Core.Comun;
using Despacho.Core.Datos;

namespace Despacho.Core.Auditoria;

public static class AuditoriaExtensiones
{
    public const int LargoMaximoValor = 600;

    public static RegistroAuditoria RegistrarAuditoria(
        this CoreDbContext db,
        IReloj reloj,
        int usuarioId,
        string accion,
        string entidad,
        int entidadId,
        string valorAnterior,
        string valorNuevo)
    {
        return Agregar(db, reloj, usuarioId, accion, entidad, entidadId, valorAnterior, valorNuevo, null);
    }

    public static RegistroAuditoria RegistrarAuditoriaDeNuevo(
        this CoreDbContext db,
        IReloj reloj,
        int usuarioId,
        string accion,
        string entidad,
        object entidadNueva,
        string valorAnterior,
        string valorNuevo)
    {
        return Agregar(db, reloj, usuarioId, accion, entidad, 0, valorAnterior, valorNuevo, entidadNueva);
    }

    private static RegistroAuditoria Agregar(
        CoreDbContext db,
        IReloj reloj,
        int usuarioId,
        string accion,
        string entidad,
        int entidadId,
        string valorAnterior,
        string valorNuevo,
        object? referencia)
    {
        if (string.IsNullOrWhiteSpace(accion) || string.IsNullOrWhiteSpace(entidad)
            || string.IsNullOrWhiteSpace(valorAnterior) || string.IsNullOrWhiteSpace(valorNuevo))
            throw new ArgumentException("Todos los campos del registro de auditoría son obligatorios.");

        var registro = new RegistroAuditoria
        {
            UsuarioId = usuarioId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            FechaUtc = reloj.AhoraUtc,
            ValorAnterior = Recortar(valorAnterior),
            ValorNuevo = Recortar(valorNuevo),
            EntidadReferencia = referencia
        };

        db.RegistrosAuditoria.Add(registro);
        return registro;
    }

    private static string Recortar(string valor)
    {
        return valor.Length > LargoMaximoValor ? valor.Substring(0, LargoMaximoValor) : valor;
    }
}
