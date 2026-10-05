using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;

namespace Despacho.Api.Seguridad;

public sealed class FiltroRol : IEndpointFilter
{
    private readonly Rol _rolRequerido;

    public const string ClaveUsuario = "UsuarioActual";

    public FiltroRol(Rol rolRequerido)
    {
        _rolRequerido = rolRequerido;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext contexto, EndpointFilterDelegate siguiente)
    {
        var encabezado = contexto.HttpContext.Request.Headers.Authorization.ToString();
        string? token = encabezado.StartsWith("Bearer ") ? encabezado.Substring(7).Trim() : null;

        var servicio = contexto.HttpContext.RequestServices.GetRequiredService<IServicioSesion>();
        var usuario = await servicio.ObtenerUsuarioAsync(token);

        if (usuario is null)
            return Results.Json(new { error = "Sesión no válida." }, statusCode: StatusCodes.Status401Unauthorized);

        if (usuario.Rol != _rolRequerido)
            return Results.Json(new { error = "No tienes permiso para esta operación." }, statusCode: StatusCodes.Status403Forbidden);

        contexto.HttpContext.Items[ClaveUsuario] = usuario;
        return await siguiente(contexto);
    }
}

public static class ExtensionesRol
{
    public static TBuilder RequiereRol<TBuilder>(this TBuilder builder, Rol rol) where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new FiltroRol(rol));
}
