using Despacho.Api.Seguridad;
using Despacho.Core.Documentos;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class DocumentosEndpoints
{
    public static IEndpointRouteBuilder MapDocumentos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/documentos").WithTags("Documentos");

        grupo.MapPost("/", async (IFormFile? archivo, HttpContext http, IServicioDocumentos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.SubirAsync(
                usuario.Id,
                archivo?.FileName,
                archivo?.ContentType,
                archivo?.Length ?? 0,
                archivo?.OpenReadStream());

            if (resultado.Exito)
                return Results.Ok(new { id = resultado.Valor, mensaje = "Documento subido." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion().DisableAntiforgery();

        grupo.MapGet("/", async (HttpContext http, IServicioDocumentos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var esAdministrador = usuario.Rol == Rol.Administrador;
            return Results.Ok(await servicio.ListarAsync(usuario.Id, esAdministrador));
        }).RequiereSesion();

        grupo.MapGet("/{id:int}/descargar", async (int id, HttpContext http, IServicioDocumentos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var esAdministrador = usuario.Rol == Rol.Administrador;
            var resultado = await servicio.DescargarAsync(usuario.Id, esAdministrador, id);

            if (resultado.Exito)
                return Results.File(resultado.Valor!.Contenido, resultado.Valor.TipoContenido, resultado.Valor.Nombre);
            if (resultado.Error == ServicioDocumentos.MensajeSinPermiso)
                return Results.Json(new { error = resultado.Error }, statusCode: 403);
            return Results.NotFound(new { error = resultado.Error });
        }).RequiereSesion();

        grupo.MapDelete("/{id:int}", async (int id, HttpContext http, IServicioDocumentos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var esAdministrador = usuario.Rol == Rol.Administrador;
            var resultado = await servicio.EliminarAsync(usuario.Id, esAdministrador, id);

            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Documento eliminado." });
            if (resultado.Error == ServicioDocumentos.MensajeSinPermiso)
                return Results.Json(new { error = resultado.Error }, statusCode: 403);
            return Results.NotFound(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
