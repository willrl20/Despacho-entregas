namespace Despacho.Negocio.Pedidos;

public sealed record PedidoResumen(int Id, int TiendaId, int ClienteId, int? RepartidorId, string Estado, DateTime CreadoEnUtc, decimal Total);
