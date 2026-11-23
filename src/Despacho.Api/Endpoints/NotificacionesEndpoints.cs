using Despacho.Api.Contratos;
using Despacho.Api.Seguridad;
using Despacho.Core.Notificaciones;
using Despacho.Core.Sesiones;

namespace Despacho.Api.Endpoints;

public static class NotificacionesEndpoints
{
    public static IEndpointRouteBuilder MapNotificaciones(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/notificaciones").WithTags("Notificaciones");

        grupo.MapGet("/", async (bool? soloNoLeidas, HttpContext http, IServicioNotificaciones servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var lista = await servicio.ListarAsync(usuario.Id, soloNoLeidas ?? false);
            return Results.Ok(lista.Select(n => new { n.Id, n.Titulo, n.Mensaje, leida = n.Leida, n.CreadaEnUtc }));
        }).RequiereSesion();

        grupo.MapPost("/{id:int}/leida", async (int id, HttpContext http, IServicioNotificaciones servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.MarcarLeidaAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Notificación marcada como leída." });
            return Results.NotFound(new { error = resultado.Error });
        }).RequiereSesion();

        grupo.MapPut("/preferencias", async (SolicitudPreferencias? solicitud, HttpContext http, IServicioNotificaciones servicio) =>
        {
            if (solicitud?.Correo is null)
                return Results.BadRequest(new { error = "Indica si quieres recibir correos (correo: true o false)." });

            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CambiarPreferenciaCorreoAsync(usuario.Id, solicitud.Correo.Value);
            if (resultado.Exito)
                return Results.Ok(new { correo = solicitud.Correo.Value });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
