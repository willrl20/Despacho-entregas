namespace Despacho.Negocio.Envios;

public static class TarifaEnvio
{
    public const decimal TarifaBase = 100m;
    public const double KmIncluidos = 2;
    public const decimal PrecioPorKmExtra = 20m;
    public const double DistanciaMaximaKm = 30;

    public static double CalcularDistanciaKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double radioTierraKm = 6371;
        var dLat = ARadianes(lat2 - lat1);
        var dLng = ARadianes(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ARadianes(lat1)) * Math.Cos(ARadianes(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(radioTierraKm * c, 2);
    }

    public static decimal Multiplicador(TamanoPaquete tamano) => tamano switch
    {
        TamanoPaquete.Pequeno => 1.00m,
        TamanoPaquete.Mediano => 1.25m,
        TamanoPaquete.Grande => 1.50m,
        _ => throw new ArgumentOutOfRangeException(nameof(tamano))
    };

    public static decimal CalcularCosto(double distanciaKm, TamanoPaquete tamano)
    {
        var baseCosto = TarifaBase + PrecioPorKmExtra * (decimal)Math.Max(0, distanciaKm - KmIncluidos);
        return Math.Round(baseCosto * Multiplicador(tamano), 2);
    }

    private static double ARadianes(double grados) => grados * Math.PI / 180;
}
