using Despacho.Api.Contratos;
using Despacho.Core.Sesiones;

namespace Despacho.Api.Endpoints;

public static class SesionEndpoints
{
    private static string? LeerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer "))
            return header.Substring(7).Trim();
        return null;
    }

    public static IEndpointRouteBuilder MapSesion(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/sesion").WithTags("Sesion");

        grupo.MapPost("/iniciar", async (SolicitudInicioSesion? solicitud, IServicioSesion servicio) =>
        {
            var resultado = await servicio.IniciarAsync(solicitud?.Correo, solicitud?.Contrasena);
            if (resultado.Exito)
                return Results.Ok(new { token = resultado.Valor });
            return Results.Json(new { error = resultado.Error }, statusCode: StatusCodes.Status401Unauthorized);
        });

        grupo.MapGet("/yo", async (HttpRequest request, IServicioSesion servicio) =>
        {
            var usuario = await servicio.ObtenerUsuarioAsync(LeerToken(request));
            if (usuario is null)
                return Results.Json(new { error = "Sesión no válida." }, statusCode: 401);
            return Results.Ok(new { id = usuario.Id, correo = usuario.Correo, rol = usuario.Rol.ToString() });
        });

        grupo.MapPost("/cerrar", async (HttpRequest request, IServicioSesion servicio) =>
        {
            await servicio.CerrarAsync(LeerToken(request));
            return Results.Ok(new { mensaje = "Sesión cerrada." });
        });

        return app;
    }
}
