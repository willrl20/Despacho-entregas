namespace Despacho.Core.Correos;

public interface IRemitenteCorreo
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpo);
}
