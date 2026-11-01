using Despacho.Core.Comun;
using Despacho.Core.Permisos;
using Despacho.Negocio.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Pedidos;

public sealed class ServicioPedidos : IServicioPedidos
{
    private readonly NegocioDbContext _db;
    private readonly IServicioPermisos _permisos;
    private readonly IReloj _reloj;

    private const string MensajeDatosInvalidos = "Los datos del pedido no son válidos.";
    private const string MensajeSinPermiso = "Necesitas el permiso Repartidor aprobado.";
    private const string MensajeNoExiste = "El pedido no existe.";
    private const string MensajePropio = "No puedes aceptar tu propio pedido.";
    private const string MensajeNoDisponible = "El pedido ya no está disponible.";
    private const string MensajeNoEsTuPedido = "No eres el repartidor de este pedido.";
    private const string MensajeTransicionInvalida = "El pedido no puede cambiar a ese estado.";
    private const string MensajeNoPuedeCancelar = "Solo el cliente del pedido o un Administrador pueden cancelarlo.";
    private const string MensajeSinDestino = "No puedes ver el destino de este pedido.";

    public ServicioPedidos(NegocioDbContext db, IServicioPermisos permisos, IReloj reloj)
    {
        _db = db;
        _permisos = permisos;
        _reloj = reloj;
    }

    public async Task<Resultado<int>> RegistrarAsync(int clienteId, int tiendaId, double latitud, double longitud, string? referencia, IReadOnlyList<LineaNueva>? lineas)
    {
        if (tiendaId <= 0)
            return Resultado<int>.Falla(MensajeDatosInvalidos);

        if (latitud < -90 || latitud > 90)
            return Resultado<int>.Falla(MensajeDatosInvalidos);

        if (longitud < -180 || longitud > 180)
            return Resultado<int>.Falla(MensajeDatosInvalidos);

        if (referencia != null && referencia.Length > 200)
            return Resultado<int>.Falla(MensajeDatosInvalidos);

        if (lineas == null || lineas.Count == 0 || lineas.Count > 50)
            return Resultado<int>.Falla(MensajeDatosInvalidos);

        foreach (var linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea.Producto) || linea.Producto.Length > 100)
                return Resultado<int>.Falla(MensajeDatosInvalidos);

            if (linea.Cantidad < 1 || linea.Cantidad > 1000)
                return Resultado<int>.Falla(MensajeDatosInvalidos);

            if (linea.PrecioUnitario <= 0 || linea.PrecioUnitario > 1000000)
                return Resultado<int>.Falla(MensajeDatosInvalidos);
        }

        var pedido = new Pedido
        {
            TiendaId = tiendaId,
            DestinatarioId = clienteId,
            DestinoLatitud = latitud,
            DestinoLongitud = longitud,
            DestinoReferencia = referencia?.Trim(),
            CreadoEnUtc = _reloj.AhoraUtc
        };

        foreach (var linea in lineas)
        {
            pedido.Lineas.Add(new LineaPedido
            {
                Producto = linea.Producto!.Trim(),
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario
            });
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return Resultado<int>.Ok(pedido.Id);
    }

    public async Task<Resultado<IReadOnlyList<PedidoResumen>>> ListarDisponiblesAsync(int repartidorId)
    {
        if (!await _permisos.TienePermisoAsync(repartidorId, Permiso.Repartidor))
            return Resultado<IReadOnlyList<PedidoResumen>>.Falla(MensajeSinPermiso);

        var pedidos = await _db.Pedidos
            .Include(p => p.Lineas)
            .Where(p => p.Estado == EstadoPedido.Pendiente)
            .OrderBy(p => p.CreadoEnUtc)
            .Select(p => new PedidoResumen(
                p.Id,
                p.TiendaId,
                p.DestinatarioId,
                p.RepartidorId,
                p.Estado.ToString(),
                p.CreadoEnUtc,
                p.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario)))
            .ToListAsync();

        return Resultado<IReadOnlyList<PedidoResumen>>.Ok(pedidos);
    }

    public async Task<Resultado> AceptarAsync(int repartidorId, int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
            return Resultado.Falla(MensajeNoExiste);

        if (pedido.DestinatarioId == repartidorId)
            return Resultado.Falla(MensajePropio);

        if (!pedido.CambiarEstado(EstadoPedido.Asignado))
            return Resultado.Falla(MensajeNoDisponible);

        pedido.RepartidorId = repartidorId;
        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado<DestinoPedido>> VerDestinoAsync(int repartidorId, int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
            return Resultado<DestinoPedido>.Falla(MensajeNoExiste);

        if (pedido.RepartidorId != repartidorId || (pedido.Estado != EstadoPedido.Asignado && pedido.Estado != EstadoPedido.EnRuta))
            return Resultado<DestinoPedido>.Falla(MensajeSinDestino);

        return Resultado<DestinoPedido>.Ok(new DestinoPedido(pedido.DestinoLatitud, pedido.DestinoLongitud, pedido.DestinoReferencia));
    }

    public async Task<Resultado> MarcarEnRutaAsync(int repartidorId, int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
            return Resultado.Falla(MensajeNoExiste);

        if (pedido.RepartidorId != repartidorId)
            return Resultado.Falla(MensajeNoEsTuPedido);

        if (!pedido.CambiarEstado(EstadoPedido.EnRuta))
            return Resultado.Falla(MensajeTransicionInvalida);

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado> ConfirmarEntregaAsync(int repartidorId, int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
            return Resultado.Falla(MensajeNoExiste);

        if (pedido.RepartidorId != repartidorId)
            return Resultado.Falla(MensajeNoEsTuPedido);

        if (!pedido.CambiarEstado(EstadoPedido.Entregado))
            return Resultado.Falla(MensajeTransicionInvalida);

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<Resultado> CancelarAsync(int usuarioId, bool esAdministrador, int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
            return Resultado.Falla(MensajeNoExiste);

        if (!esAdministrador && pedido.DestinatarioId != usuarioId)
            return Resultado.Falla(MensajeNoPuedeCancelar);

        if (!pedido.CambiarEstado(EstadoPedido.Cancelado))
            return Resultado.Falla(MensajeTransicionInvalida);

        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<PedidoResumen>> ListarDeClienteAsync(int clienteId)
    {
        return await _db.Pedidos
            .Include(p => p.Lineas)
            .Where(p => p.DestinatarioId == clienteId)
            .OrderByDescending(p => p.CreadoEnUtc)
            .Select(p => new PedidoResumen(
                p.Id,
                p.TiendaId,
                p.DestinatarioId,
                p.RepartidorId,
                p.Estado.ToString(),
                p.CreadoEnUtc,
                p.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario)))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PedidoResumen>> ListarTodosAsync()
    {
        return await _db.Pedidos
            .Include(p => p.Lineas)
            .OrderByDescending(p => p.CreadoEnUtc)
            .Select(p => new PedidoResumen(
                p.Id,
                p.TiendaId,
                p.DestinatarioId,
                p.RepartidorId,
                p.Estado.ToString(),
                p.CreadoEnUtc,
                p.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario)))
            .ToListAsync();
    }
}
