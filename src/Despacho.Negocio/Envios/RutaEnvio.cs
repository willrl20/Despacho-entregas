namespace Despacho.Negocio.Envios;

public sealed record RutaEnvio(int Id, string Modalidad, string Estado, double OrigenLatitud, double OrigenLongitud, string? OrigenReferencia, double DestinoLatitud, double DestinoLongitud, string? DestinoReferencia, string ContactoNombre, string ContactoTelefono, string DescripcionPaquete, string Tamano);
