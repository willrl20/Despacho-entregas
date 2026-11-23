using Despacho.Api.Seguridad;
using Despacho.Core.Correos;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class CorreosEndpoints
{
    public static IEndpointRouteBuilder MapCorreos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/correos").WithTags("Correos");

        grupo.MapGet("/", async (string? estado, IServicioCorreos servicio) =>
        {
            var resultado = await servicio.ListarAsync(estado);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Administrador);

        return app;
    }
}
