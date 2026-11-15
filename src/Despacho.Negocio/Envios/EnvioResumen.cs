namespace Despacho.Negocio.Envios;

public sealed record EnvioResumen(int Id, string Modalidad, string Estado, string DescripcionPaquete, string Tamano, double DistanciaKm, decimal Costo, int? RepartidorId, DateTime CreadoEnUtc);
