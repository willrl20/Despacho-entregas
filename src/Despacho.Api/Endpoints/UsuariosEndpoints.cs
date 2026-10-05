using Despacho.Api.Contratos;
using Despacho.Api.Seguridad;
using Despacho.Core.Administracion;
using Despacho.Core.Contrasenas;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static IEndpointRouteBuilder MapUsuarios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuarios").WithTags("Usuarios").RequiereRol(Rol.Administrador);

        grupo.MapGet("/", async (IServicioAdministracion servicio) =>
        {
            return Results.Ok(await servicio.ListarAsync());
        });

        grupo.MapPut("/{id:int}/rol", async (int id, SolicitudCambioRol? solicitud, IServicioAdministracion servicio) =>
        {
            var resultado = await servicio.CambiarRolAsync(id, solicitud?.Rol);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Rol actualizado." });
            return Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapPost("/{id:int}/desactivar", async (int id, HttpContext http, IServicioAdministracion servicio) =>
        {
            var admin = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.DesactivarAsync(admin.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Usuario desactivado." });
            return Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapPost("/{id:int}/reactivar", async (int id, IServicioAdministracion servicio) =>
        {
            var resultado = await servicio.ReactivarAsync(id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Usuario reactivado." });
            return Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapPost("/{id:int}/forzar-restablecimiento", async (int id, IServicioContrasenas servicio) =>
        {
            var resultado = await servicio.ForzarRestablecimientoAsync(id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Se envió un código de restablecimiento al usuario." });
            return Results.BadRequest(new { error = resultado.Error });
        });

        return app;
    }
}
