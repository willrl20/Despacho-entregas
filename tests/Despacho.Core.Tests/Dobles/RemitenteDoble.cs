using Despacho.Core.Correos;

namespace Despacho.Core.Tests.Dobles;

public sealed class RemitenteDoble : IRemitenteCorreo
{
    private readonly Exception? _falla;

    public RemitenteDoble(Exception? falla = null)
    {
        _falla = falla;
    }

    public int Llamadas { get; private set; }

    public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        Llamadas++;
        if (_falla != null)
            throw _falla;
        return Task.CompletedTask;
    }
}
