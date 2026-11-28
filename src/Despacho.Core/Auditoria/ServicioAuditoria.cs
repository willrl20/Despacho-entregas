using System.Globalization;
using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Auditoria;

public sealed class ServicioAuditoria : IServicioAuditoria
{
    public const string MensajeSinPermiso = "No tienes permiso para consultar el historial.";
    public const string MensajeUsuarioInvalido = "El usuarioId debe ser un número entero positivo.";
    public const string MensajeEntidadInvalida = "La entidad debe ser SolicitudPermiso, Documento o Usuario.";

    private const string FechaMinima = "0001-01-01";
    private const string FechaMaxima = "9999-12-31";

    private static readonly string[] Entidades =
    {
        EntidadesAuditoria.SolicitudPermiso,
        EntidadesAuditoria.Documento,
        EntidadesAuditoria.Usuario
    };

    private readonly CoreDbContext _db;

    public ServicioAuditoria(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<Resultado<IReadOnlyList<RegistroAuditoriaResumen>>> ConsultarAsync(UsuarioActual consultante, string? usuarioId, string? entidad, string? desde, string? hasta)
    {
        if (consultante.Rol != Rol.Administrador)
            return Resultado<IReadOnlyList<RegistroAuditoriaResumen>>.Falla(MensajeSinPermiso);

        int? idFiltro = null;
        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            if (!int.TryParse(usuarioId.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                return Resultado<IReadOnlyList<RegistroAuditoriaResumen>>.Falla(MensajeUsuarioInvalido);
            idFiltro = id;
        }

        string? entidadFiltro = null;
        if (!string.IsNullOrWhiteSpace(entidad))
        {
            entidadFiltro = Entidades.FirstOrDefault(e => string.Equals(e, entidad.Trim(), StringComparison.OrdinalIgnoreCase));
            if (entidadFiltro is null)
                return Resultado<IReadOnlyList<RegistroAuditoriaResumen>>.Falla(MensajeEntidadInvalida);
        }

        RangoFechas? rango = null;
        if (!string.IsNullOrWhiteSpace(desde) || !string.IsNullOrWhiteSpace(hasta))
        {
            var resultadoRango = RangoFechas.Crear(
                string.IsNullOrWhiteSpace(desde) ? FechaMinima : desde,
                string.IsNullOrWhiteSpace(hasta) ? FechaMaxima : hasta);

            if (!resultadoRango.Exito)
                return Resultado<IReadOnlyList<RegistroAuditoriaResumen>>.Falla(resultadoRango.Error!);

            rango = resultadoRango.Valor;
        }

        var consulta = _db.RegistrosAuditoria.AsQueryable();

        if (idFiltro.HasValue)
            consulta = consulta.Where(r => r.UsuarioId == idFiltro.Value);

        if (entidadFiltro is not null)
            consulta = consulta.Where(r => r.Entidad == entidadFiltro);

        if (rango is not null)
        {
            var inicio = rango.DesdeUtc;
            var fin = rango.HastaExclusivoUtc;
            consulta = consulta.Where(r => r.FechaUtc >= inicio && r.FechaUtc < fin);
        }

        IReadOnlyList<RegistroAuditoriaResumen> registros = await consulta
            .OrderByDescending(r => r.FechaUtc)
            .ThenByDescending(r => r.Id)
            .Select(r => new RegistroAuditoriaResumen(r.Id, r.UsuarioId, r.Accion, r.Entidad, r.EntidadId, r.FechaUtc, r.ValorAnterior, r.ValorNuevo))
            .ToListAsync();

        return Resultado<IReadOnlyList<RegistroAuditoriaResumen>>.Ok(registros);
    }
}
