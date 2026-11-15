using Despacho.Core.Comun;

namespace Despacho.Negocio.Envios;

public interface IServicioEnvios
{
    Resultado<Cotizacion> Cotizar(DatosEnvio? datos);
    Task<Resultado<int>> CrearAsync(int clienteId, DatosEnvio? datos);
    Task<IReadOnlyList<EnvioResumen>> ListarDeClienteAsync(int clienteId);
    Task<IReadOnlyList<EnvioResumen>> ListarTodosAsync();
    Task<Resultado> CancelarAsync(int usuarioId, bool esAdministrador, int envioId);
    Task<Resultado<IReadOnlyList<EnvioDisponible>>> ListarDisponiblesAsync(int repartidorId);
    Task<Resultado> AceptarAsync(int repartidorId, int envioId);
    Task<Resultado<RutaEnvio>> VerRutaAsync(int repartidorId, int envioId);
    Task<Resultado> MarcarRecogidoAsync(int repartidorId, int envioId);
    Task<Resultado> ConfirmarEntregaAsync(int repartidorId, int envioId);
}
