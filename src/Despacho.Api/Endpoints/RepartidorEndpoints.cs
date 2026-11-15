using Despacho.Api.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Direcciones;
using Despacho.Negocio.Envios;

namespace Despacho.Api.Endpoints;

public static class RepartidorEndpoints
{
    public static IEndpointRouteBuilder MapRepartidor(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/repartidor").WithTags("Repartidor");

        grupo.MapGet("/envios-disponibles", async (HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ListarDisponiblesAsync(usuario.Id);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.Json(new { error = resultado.Error }, statusCode: StatusCodes.Status403Forbidden);
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/envios/{id:int}/aceptar", async (int id, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.AceptarAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Envío aceptado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/envios/{id:int}/ruta", async (int id, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.VerRutaAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/envios/{id:int}/direcciones", async (int id, HttpContext http, IServicioDirecciones servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.VerDireccionesAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            if (resultado.Error == GeocodificadorOpenCage.MensajeNoConfigurado || resultado.Error == GeocodificadorOpenCage.MensajeNoDisponible)
                return Results.Json(new { error = resultado.Error }, statusCode: StatusCodes.Status503ServiceUnavailable);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/envios/{id:int}/recoger", async (int id, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.MarcarRecogidoAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Paquete recogido. Envío en ruta." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/envios/{id:int}/entregar", async (int id, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ConfirmarEntregaAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Envío entregado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        return app;
    }
}
