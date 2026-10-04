namespace Despacho.Core.Correos;

public interface IProcesadorCola
{
    Task<ResumenEnvio> ProcesarPendientesAsync();
}
