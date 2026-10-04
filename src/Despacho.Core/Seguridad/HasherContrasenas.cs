using Microsoft.AspNetCore.Identity;

namespace Despacho.Core.Seguridad;

public sealed class HasherContrasenas : IHasherContrasenas
{
    private static readonly object UsuarioVacio = new object();

    private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

    public string Hashear(string contrasena)
    {
        return _hasher.HashPassword(UsuarioVacio, contrasena);
    }

    public bool Verificar(string hash, string contrasena)
    {
        return _hasher.VerifyHashedPassword(UsuarioVacio, hash, contrasena) != PasswordVerificationResult.Failed;
    }
}