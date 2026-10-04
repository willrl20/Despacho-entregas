namespace Despacho.Core.Comun;

public sealed class Resultado
{
    public bool Exito { get; }
    public string? Error { get; }

    private Resultado(bool exito, string? error)
    {
        Exito = exito;
        Error = error;
    }

    public static Resultado Ok()
    {
        return new Resultado(true, null);
    }

    public static Resultado Falla(string error)
    {
        return new Resultado(false, error);
    }
}
