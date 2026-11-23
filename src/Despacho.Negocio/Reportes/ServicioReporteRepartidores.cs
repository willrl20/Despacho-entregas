using Despacho.Core.Comun;
using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Reportes;

public sealed class ServicioReporteRepartidores : IServicioReporteRepartidores
{
    private readonly NegocioDbContext _db;

    public ServicioReporteRepartidores(NegocioDbContext db)
    {
        _db = db;
    }

    public async Task<Resultado<IReadOnlyList<FilaReporteRepartidores>>> ObtenerAsync(UsuarioActual usuario, string? desde, string? hasta)
    {
        var rango = RangoFechas.Crear(desde, hasta);
        if (!rango.Exito)
            return Resultado<IReadOnlyList<FilaReporteRepartidores>>.Falla(rango.Error!);

        var inicio = rango.Valor!.DesdeUtc;
        var fin = rango.Valor.HastaExclusivoUtc;

        var consulta = _db.Envios.Where(e =>
            e.Estado == EstadoEnvio.Entregado
            && e.RepartidorId != null
            && e.CreadoEnUtc >= inicio
            && e.CreadoEnUtc < fin);

        if (usuario.Rol != Rol.Administrador)
            consulta = consulta.Where(e => e.RepartidorId == usuario.Id);

        var grupos = await consulta
            .GroupBy(e => e.RepartidorId!.Value)
            .Select(g => new
            {
                RepartidorId = g.Key,
                Entregados = g.Count(),
                TotalKm = g.Sum(e => e.DistanciaKm),
                TotalGenerado = g.Sum(e => e.Costo)
            })
            .OrderByDescending(f => f.TotalGenerado)
            .ThenBy(f => f.RepartidorId)
            .ToListAsync();

        IReadOnlyList<FilaReporteRepartidores> filas = grupos
            .Select(g => new FilaReporteRepartidores(g.RepartidorId, g.Entregados, g.TotalKm, g.TotalGenerado))
            .ToList();

        return Resultado<IReadOnlyList<FilaReporteRepartidores>>.Ok(filas);
    }
}
