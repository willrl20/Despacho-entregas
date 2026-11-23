using System.Globalization;
using Despacho.Core.Comun;

namespace Despacho.Core.Reportes;

public sealed record RangoFechas(DateTime DesdeUtc, DateTime HastaExclusivoUtc)
{
    public const string FormatoFecha = "yyyy-MM-dd";
    public const string MensajeFechaObligatoria = "Indica las fechas desde y hasta en formato AAAA-MM-DD.";
    public const string MensajeFechaInvalida = "Las fechas deben existir y tener el formato AAAA-MM-DD.";
    public const string MensajeRangoInvertido = "La fecha desde no puede ser posterior a la fecha hasta.";

    public static Resultado<RangoFechas> Crear(string? desde, string? hasta)
    {
        if (string.IsNullOrWhiteSpace(desde) || string.IsNullOrWhiteSpace(hasta))
            return Resultado<RangoFechas>.Falla(MensajeFechaObligatoria);

        if (!DateOnly.TryParseExact(desde.Trim(), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaDesde)
            || !DateOnly.TryParseExact(hasta.Trim(), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaHasta))
            return Resultado<RangoFechas>.Falla(MensajeFechaInvalida);

        if (fechaDesde > fechaHasta)
            return Resultado<RangoFechas>.Falla(MensajeRangoInvertido);

        var inicio = DateTime.SpecifyKind(fechaDesde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var fin = fechaHasta == DateOnly.MaxValue
            ? DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)
            : DateTime.SpecifyKind(fechaHasta.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        return Resultado<RangoFechas>.Ok(new RangoFechas(inicio, fin));
    }
}
