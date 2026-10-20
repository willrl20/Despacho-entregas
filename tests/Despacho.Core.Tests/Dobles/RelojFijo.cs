using Despacho.Core.Comun;

namespace Despacho.Core.Tests.Dobles;

public sealed class RelojFijo : IReloj
{
    public static readonly DateTime Ahora = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);

    public DateTime AhoraUtc => Ahora;
}
