using System;
using System.Security.Cryptography;

namespace Despacho.Core.Seguridad;

public sealed class GeneradorTokens : IGeneradorTokens
{
    public string Generar()
    {
           var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }
}