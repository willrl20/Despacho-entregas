using System;

namespace Despacho.Core.Comun;

public interface IReloj
{
    DateTime AhoraUtc { get; }
}