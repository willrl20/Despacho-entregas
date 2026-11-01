using Despacho.Core.Comun;

namespace Despacho.Negocio.Tests.Dobles;

public sealed class RelojFijo : IReloj
{
    public static DateTime Ahora { get; } = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    public DateTime AhoraUtc => Ahora;
}
