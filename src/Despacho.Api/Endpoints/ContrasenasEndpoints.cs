using Despacho.Api.Contratos;
using Despacho.Api.Seguridad;
using Despacho.Core.Contrasenas;
using Despacho.Core.Sesiones;

namespace Despacho.Api.Endpoints;

public static class ContrasenasEndpoints
{
    public static IEndpointRouteBuilder MapContrasenas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/contrasenas").WithTags("Contrasenas");

        grupo.MapPost("/recuperar", async (SolicitudRecuperacion? solicitud, IServicioContrasenas servicio) =>
        {
            await servicio.SolicitarRecuperacionAsync(solicitud?.Correo);
            return Results.Ok(new { mensaje = "Si el correo está registrado, te enviamos un código para restablecer la contraseña." });
        });

        grupo.MapPost("/restablecer", async (SolicitudRestablecimiento? solicitud, IServicioContrasenas servicio) =>
        {
            var resultado = await servicio.RestablecerAsync(solicitud?.Correo, solicitud?.Codigo, solicitud?.ContrasenaNueva);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Contraseña actualizada. Inicia sesión de nuevo." });
            return Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapPost("/cambiar", async (SolicitudCambioContrasena? solicitud, HttpContext http, IServicioContrasenas servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CambiarAsync(usuario.Id, solicitud?.ContrasenaActual, solicitud?.ContrasenaNueva);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Contraseña actualizada. Inicia sesión de nuevo." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
