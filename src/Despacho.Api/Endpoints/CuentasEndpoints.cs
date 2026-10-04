using Despacho.Api.Contratos;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Endpoints;

public static class CuentasEndpoints
{
    public static IEndpointRouteBuilder MapCuentas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/cuentas").WithTags("Cuentas");

        grupo.MapPost("/registro", async (SolicitudRegistro? solicitud, IServicioRegistro servicio) =>
        {
            var resultado = await servicio.RegistrarAsync(solicitud?.Correo, solicitud?.Contrasena);
            return resultado.Exito
                ? Results.Ok(new { mensaje = "Cuenta creada. Revisa tu correo para activarla." })
                : Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapGet("/activar", async (string? token, IServicioRegistro servicio) =>
        {
            var resultado = await servicio.ActivarAsync(token);
            return resultado.Exito
                ? Results.Ok(new { mensaje = "Cuenta activada. Ya puedes iniciar sesión." })
                : Results.BadRequest(new { error = resultado.Error });
        });

        grupo.MapPost("/reenviar-activacion", async (SolicitudReenvio? solicitud, IServicioRegistro servicio) =>
        {
            await servicio.ReenviarActivacionAsync(solicitud?.Correo);
            return Results.Ok(new { mensaje = "Si el correo está registrado y la cuenta no está activa, te enviamos un nuevo enlace." });
        });

        return app;
    }
}
