using Despacho.Negocio.Pedidos;

namespace Despacho.Api.Contratos;

public sealed record SolicitudRegistroPedido(int TiendaId, double Latitud, double Longitud, string? Referencia, List<LineaNueva>? Lineas);
