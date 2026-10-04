using System;

namespace Despacho.Core.Comun;

public sealed class RelojSistema : IReloj
{
    public DateTime AhoraUtc => DateTime.UtcNow;
}