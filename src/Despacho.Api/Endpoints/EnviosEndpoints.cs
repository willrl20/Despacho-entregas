using Despacho.Api.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Envios;

namespace Despacho.Api.Endpoints;

public static class EnviosEndpoints
{
    public static IEndpointRouteBuilder MapEnvios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/envios").WithTags("Envíos");

        grupo.MapPost("/cotizacion", (DatosEnvio? datos, IServicioEnvios servicio) =>
        {
            var resultado = servicio.Cotizar(datos);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/", async (DatosEnvio? datos, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CrearAsync(usuario.Id, datos);
            if (resultado.Exito)
                return Results.Ok(new { id = resultado.Valor, mensaje = "Envío creado. Queda pendiente." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/mios", async (HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            return Results.Ok(await servicio.ListarDeClienteAsync(usuario.Id));
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/", async (IServicioEnvios servicio) =>
        {
            return Results.Ok(await servicio.ListarTodosAsync());
        }).RequiereRol(Rol.Administrador);

        grupo.MapPost("/{id:int}/cancelar", async (int id, HttpContext http, IServicioEnvios servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CancelarAsync(usuario.Id, usuario.Rol == Rol.Administrador, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Envío cancelado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
