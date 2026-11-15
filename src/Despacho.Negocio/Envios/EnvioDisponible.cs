namespace Despacho.Negocio.Envios;

public sealed record EnvioDisponible(int Id, string Tamano, double DistanciaKm, decimal Costo, DateTime CreadoEnUtc);
