using Despacho.Api.Seguridad;
using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Negocio.Reportes;

namespace Despacho.Api.Endpoints;

public static class ReportesEndpoints
{
    public static IEndpointRouteBuilder MapReportes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/reportes").WithTags("Reportes");

        grupo.MapGet("/solicitudes", async (string? desde, string? hasta, HttpContext http, IServicioReporteSolicitudes servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ObtenerAsync(usuario, desde, hasta);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        grupo.MapGet("/repartidores", async (string? desde, string? hasta, HttpContext http, IServicioReporteRepartidores servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ObtenerAsync(usuario, desde, hasta);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
