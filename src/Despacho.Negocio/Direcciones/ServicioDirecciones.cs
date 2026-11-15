using Despacho.Core.Comun;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Envios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Direcciones;

public sealed class ServicioDirecciones : IServicioDirecciones
{
    private readonly NegocioDbContext _db;
    private readonly IGeocodificador _geocodificador;

    public const string MensajeNoExiste = "El envío no existe.";
    public const string MensajeSinRuta = "No puedes ver la ruta de este envío.";

    public ServicioDirecciones(NegocioDbContext db, IGeocodificador geocodificador)
    {
        _db = db;
        _geocodificador = geocodificador;
    }

    public async Task<Resultado<DireccionesEnvio>> VerDireccionesAsync(int repartidorId, int envioId)
    {
        var envio = await _db.Envios.FirstOrDefaultAsync(e => e.Id == envioId);
        if (envio == null)
            return Resultado<DireccionesEnvio>.Falla(MensajeNoExiste);

        if (envio.RepartidorId != repartidorId
            || (envio.Estado != EstadoEnvio.Asignado && envio.Estado != EstadoEnvio.EnRuta))
            return Resultado<DireccionesEnvio>.Falla(MensajeSinRuta);

        var origen = await _geocodificador.ObtenerDireccionAsync(envio.OrigenLatitud, envio.OrigenLongitud);
        if (!origen.Exito)
            return Resultado<DireccionesEnvio>.Falla(origen.Error!);

        var destino = await _geocodificador.ObtenerDireccionAsync(envio.DestinoLatitud, envio.DestinoLongitud);
        if (!destino.Exito)
            return Resultado<DireccionesEnvio>.Falla(destino.Error!);

        return Resultado<DireccionesEnvio>.Ok(new DireccionesEnvio(origen.Valor!, destino.Valor!));
    }
}
