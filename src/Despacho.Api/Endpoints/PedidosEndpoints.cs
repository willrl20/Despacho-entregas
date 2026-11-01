using Despacho.Api.Contratos;
using Despacho.Api.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Pedidos;

namespace Despacho.Api.Endpoints;

public static class PedidosEndpoints
{
    public static IEndpointRouteBuilder MapPedidos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/pedidos").WithTags("Pedidos");

        grupo.MapPost("/", async (SolicitudRegistroPedido? solicitud, HttpContext http, IServicioPedidos servicio) =>
        {
            if (solicitud == null)
                return Results.BadRequest(new { error = "Los datos del pedido no son válidos." });

            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.RegistrarAsync(usuario.Id, solicitud.TiendaId, solicitud.Latitud, solicitud.Longitud, solicitud.Referencia, solicitud.Lineas);
            if (resultado.Exito)
                return Results.Ok(new { id = resultado.Valor, mensaje = "Pedido registrado. Queda pendiente." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/mios", async (HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            return Results.Ok(await servicio.ListarDeClienteAsync(usuario.Id));
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/", async (IServicioPedidos servicio) =>
        {
            return Results.Ok(await servicio.ListarTodosAsync());
        }).RequiereRol(Rol.Administrador);

        grupo.MapGet("/disponibles", async (HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ListarDisponiblesAsync(usuario.Id);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.Json(new { error = resultado.Error }, statusCode: StatusCodes.Status403Forbidden);
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/{id:int}/aceptar", async (int id, HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.AceptarAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Pedido aceptado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapGet("/{id:int}/destino", async (int id, HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.VerDestinoAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(resultado.Valor);
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/{id:int}/en-ruta", async (int id, HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.MarcarEnRutaAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Pedido en ruta." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/{id:int}/entregar", async (int id, HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.ConfirmarEntregaAsync(usuario.Id, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Pedido entregado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereRol(Rol.Estandar);

        grupo.MapPost("/{id:int}/cancelar", async (int id, HttpContext http, IServicioPedidos servicio) =>
        {
            var usuario = (UsuarioActual)http.Items[FiltroRol.ClaveUsuario]!;
            var resultado = await servicio.CancelarAsync(usuario.Id, usuario.Rol == Rol.Administrador, id);
            if (resultado.Exito)
                return Results.Ok(new { mensaje = "Pedido cancelado." });
            return Results.BadRequest(new { error = resultado.Error });
        }).RequiereSesion();

        return app;
    }
}
