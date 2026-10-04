using System;

namespace Despacho.Core.Seguridad;

public interface IHasherContrasenas
{
    string Hashear(string contrasena);

    bool Verificar(string hash, string contrasena);
}