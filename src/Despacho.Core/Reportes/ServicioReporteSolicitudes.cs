using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Reportes;

public sealed class ServicioReporteSolicitudes : IServicioReporteSolicitudes
{
    private readonly CoreDbContext _db;

    public ServicioReporteSolicitudes(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<Resultado<IReadOnlyList<FilaReporteSolicitudes>>> ObtenerAsync(UsuarioActual usuario, string? desde, string? hasta)
    {
        var rango = RangoFechas.Crear(desde, hasta);
        if (!rango.Exito)
            return Resultado<IReadOnlyList<FilaReporteSolicitudes>>.Falla(rango.Error!);

        var inicio = rango.Valor!.DesdeUtc;
        var fin = rango.Valor.HastaExclusivoUtc;

        var consulta = _db.SolicitudesPermiso.Where(s => s.CreadaEnUtc >= inicio && s.CreadaEnUtc < fin);

        if (usuario.Rol != Rol.Administrador)
            consulta = consulta.Where(s => s.SolicitanteId == usuario.Id);

        var grupos = await consulta
            .GroupBy(s => s.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        IReadOnlyList<FilaReporteSolicitudes> filas = grupos
            .OrderBy(g => g.Estado)
            .Select(g => new FilaReporteSolicitudes(g.Estado.ToString(), g.Cantidad))
            .ToList();

        return Resultado<IReadOnlyList<FilaReporteSolicitudes>>.Ok(filas);
    }
}
