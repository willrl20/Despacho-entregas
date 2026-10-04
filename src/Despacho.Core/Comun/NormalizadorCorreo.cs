namespace Despacho.Core.Comun;

public static class NormalizadorCorreo
{
    public static string? Normalizar(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return null;
        }

        var normalizado = correo.Trim().ToLowerInvariant();
        if (normalizado.Length > 256 || !System.Net.Mail.MailAddress.TryCreate(normalizado,    out var direccion) || direccion.Address != normalizado)
        {
            return null;
        }

        return normalizado;
    }
}