using Despacho.Api.Seguridad;
using Despacho.Core.Auditoria;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class AuditoriaEndpoints
{
    public static IEndpointRouteBuilder MapAuditoria(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auditoria").WithTags("Auditoría");

        grupo.MapGet("/", async (string? usuarioId, string? entidad, string? desde, string? hasta, HttpContext http, IServicioAuditoria servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ConsultarAsync(usuario, usuarioId, entidad, desde, hasta);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            if (resultado.Error == ServicioAuditoria.MensajeSinPermiso)
                return Results.Json(new { error = resultado.Error }, statusCode: StatusCodes.Status403Forbidden);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Administrador);

        return app;
    }
}
