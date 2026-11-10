using System.Globalization;
using System.Text.Json;
using Despacho.Core.Comun;

namespace Despacho.Negocio.Direcciones;

public sealed class GeocodificadorOpenCage : IGeocodificador
{
    private readonly HttpClient _http;
    private readonly OpcionesOpenCage _opciones;

    public const string MensajeNoConfigurado = "El servicio de direcciones no está configurado.";
    public const string MensajeNoDisponible = "El servicio de direcciones no está disponible. Intenta más tarde.";

    public GeocodificadorOpenCage(HttpClient http, OpcionesOpenCage opciones)
    {
        _http = http;
        _opciones = opciones;
    }

    public async Task<Resultado<string>> ObtenerDireccionAsync(double latitud, double longitud)
    {
        if (string.IsNullOrWhiteSpace(_opciones.ClaveApi))
            return Resultado<string>.Falla(MensajeNoConfigurado);

        var lat = latitud.ToString(CultureInfo.InvariantCulture);
        var lng = longitud.ToString(CultureInfo.InvariantCulture);
        var clave = Uri.EscapeDataString(_opciones.ClaveApi);
        var url = $"https://api.opencagedata.com/geocode/v1/json?q={lat}+{lng}&key={clave}&language=es&no_annotations=1&limit=1";

        try
        {
            using var respuesta = await _http.GetAsync(url);
            if (!respuesta.IsSuccessStatusCode)
                return Resultado<string>.Falla(MensajeNoDisponible);

            var json = await respuesta.Content.ReadAsStringAsync();
            using var documento = JsonDocument.Parse(json);

            if (!documento.RootElement.TryGetProperty("results", out var resultados)
                || resultados.ValueKind != JsonValueKind.Array
                || resultados.GetArrayLength() == 0)
                return Resultado<string>.Falla(MensajeNoDisponible);

            if (!resultados[0].TryGetProperty("formatted", out var formateada)
                || formateada.ValueKind != JsonValueKind.String)
                return Resultado<string>.Falla(MensajeNoDisponible);

            var direccion = formateada.GetString();
            if (string.IsNullOrWhiteSpace(direccion))
                return Resultado<string>.Falla(MensajeNoDisponible);

            return Resultado<string>.Ok(direccion);
        }
        catch (HttpRequestException)
        {
            return Resultado<string>.Falla(MensajeNoDisponible);
        }
        catch (TaskCanceledException)
        {
            return Resultado<string>.Falla(MensajeNoDisponible);
        }
        catch (JsonException)
        {
            return Resultado<string>.Falla(MensajeNoDisponible);
        }
    }
}
