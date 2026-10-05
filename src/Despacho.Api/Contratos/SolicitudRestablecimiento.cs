namespace Despacho.Api.Contratos;

public sealed record SolicitudRestablecimiento(string? Correo, string? Codigo, string? ContrasenaNueva);
