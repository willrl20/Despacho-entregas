using Despacho.Api.Contratos;
using Despacho.Api.Seguridad;
using Despacho.Core.Permisos;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class SolicitudesEndpoints
{
    public static IEndpointRouteBuilder MapSolicitudes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/solicitudes").WithTags("Solicitudes");

        grupo.MapPost("/", async (SolicitudCreacionPermiso? solicitud, HttpContext http, IServicioPermisos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CrearAsync(usuario.Id, solicitud?.Permiso);
            if (resultado.Exito)
                return Results.Ok(new { id = resultado.Valor, mensaje = "Solicitud creada. Queda pendiente." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/mias", async (HttpContext http, IServicioPermisos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            return Results.Ok(await servicio.ListarDeUsuarioAsync(usuario.Id));
        }).RequiereSesion();

        grupo.MapGet("/pendientes", async (IServicioPermisos servicio) =>
        {
            return Results.Ok(await servicio.ListarPendientesAsync());
        }).RequiereRol(Rol.Administrador);

        grupo.MapPost("/{id:int}/aprobar", async (int id, HttpContext http, IServicioPermisos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.AprobarAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Solicitud aprobada y permiso aplicado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Administrador);

        grupo.MapPost("/{id:int}/rechazar", async (int id, SolicitudRechazo? solicitud, HttpContext http, IServicioPermisos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.RechazarAsync(usuario.Id, id, solicitud?.Motivo);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Solicitud rechazada." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Administrador);

        return app;
    }
}
