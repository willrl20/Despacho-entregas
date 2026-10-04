using Despacho.Core.Correos;

namespace Despacho.EnviadorCorreos;

public sealed class RemitenteSmtp : IRemitenteCorreo
{
    private readonly OpcionesSmtp _opciones;

    public RemitenteSmtp(OpcionesSmtp opciones)
    {
        _opciones = opciones;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        using var cliente = new System.Net.Mail.SmtpClient(_opciones.Host, _opciones.Puerto)
        {
            EnableSsl = true,
            Credentials = new System.Net.NetworkCredential(_opciones.Usuario, _opciones.Contrasena)
        };
        using var mensaje = new System.Net.Mail.MailMessage(_opciones.Remitente, destinatario, asunto, cuerpo);
        await cliente.SendMailAsync(mensaje);
    }
}
