using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Correos;

public sealed class ServicioCorreos : IServicioCorreos
{
    public const string MensajeEstadoInvalido = "El estado debe ser Pendiente, Enviado o Fallido.";

    private readonly CoreDbContext _db;

    public ServicioCorreos(CoreDbContext db)
    {
        _db = db;
    }

    public async Task<Resultado<IReadOnlyList<CorreoResumen>>> ListarAsync(string? estado)
    {
        var consulta = _db.CorreosEnCola.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var texto = estado.Trim();
            if (texto.All(char.IsDigit) || !Enum.TryParse<EstadoCorreo>(texto, true, out var filtro) || !Enum.IsDefined(filtro))
                return Resultado<IReadOnlyList<CorreoResumen>>.Falla(MensajeEstadoInvalido);

            consulta = consulta.Where(c => c.Estado == filtro);
        }

        var correos = await consulta
            .OrderByDescending(c => c.CreadoEnUtc)
            .ThenByDescending(c => c.Id)
            .Select(c => new CorreoResumen(
                c.Id,
                c.Destinatario,
                c.Asunto,
                c.Estado.ToString(),
                c.Intentos,
                c.CreadoEnUtc,
                c.UltimoIntentoEnUtc,
                c.EnviadoEnUtc,
                c.UltimoError))
            .ToListAsync();

        return Resultado<IReadOnlyList<CorreoResumen>>.Ok(correos);
    }
}
