namespace Despacho.Negocio.Envios;

public static class MaquinaEstadosEnvio
{
    private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> Transiciones = new()
    {
        { EstadoEnvio.Pendiente, new[] { EstadoEnvio.Asignado, EstadoEnvio.Cancelado } },
        { EstadoEnvio.Asignado, new[] { EstadoEnvio.EnRuta, EstadoEnvio.Cancelado } },
        { EstadoEnvio.EnRuta, new[] { EstadoEnvio.Entregado } },
        { EstadoEnvio.Entregado, Array.Empty<EstadoEnvio>() },
        { EstadoEnvio.Cancelado, Array.Empty<EstadoEnvio>() }
    };

    private static readonly (EstadoEnvio Desde, EstadoEnvio Hacia)[] Prohibidas =
    {
        (EstadoEnvio.Entregado, EstadoEnvio.EnRuta)
    };

    public static bool EsTerminal(EstadoEnvio estado) => Transiciones[estado].Length == 0;

    public static bool PuedeCambiar(EstadoEnvio desde, EstadoEnvio hacia) =>
        !Prohibidas.Contains((desde, hacia)) && Transiciones[desde].Contains(hacia);
}
