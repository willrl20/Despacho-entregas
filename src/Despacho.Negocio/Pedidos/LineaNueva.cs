namespace Despacho.Negocio.Pedidos;

public sealed record LineaNueva(string? Producto, int Cantidad, decimal PrecioUnitario);
