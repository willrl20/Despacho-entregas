namespace Despacho.Negocio.Envios;

public sealed record DatosEnvio(string? Modalidad, double OrigenLatitud, double OrigenLongitud, string? OrigenReferencia, double DestinoLatitud, double DestinoLongitud, string? DestinoReferencia, string? ContactoNombre, string? ContactoTelefono, string? DescripcionPaquete, string? Tamano);
