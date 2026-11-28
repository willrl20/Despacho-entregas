namespace Despacho.Core.Correos;

public class CorreoEnCola
{
    public const int MaximoIntentos = 3;
    public const int LargoMaximoError = 500;
    public const int MinutosReserva = 5;

    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public DateTime CreadoEnUtc { get; set; }
    public EstadoCorreo Estado { get; set; } = EstadoCorreo.Pendiente;
    public int Intentos { get; set; }
    public DateTime? UltimoIntentoEnUtc { get; set; }
    public DateTime? EnviadoEnUtc { get; set; }
    public string? UltimoError { get; set; }
    public DateTime? ReservadoHastaUtc { get; set; }

    public bool EstaDisponible(DateTime ahoraUtc)
    {
        return Estado == EstadoCorreo.Pendiente && (ReservadoHastaUtc == null || ReservadoHastaUtc < ahoraUtc);
    }

    public void Reservar(DateTime ahoraUtc)
    {
        ReservadoHastaUtc = ahoraUtc.AddMinutes(MinutosReserva);
    }

    public void MarcarEnviado(DateTime ahoraUtc)
    {
        ReservadoHastaUtc = null;
        Intentos++;
        UltimoIntentoEnUtc = ahoraUtc;
        EnviadoEnUtc = ahoraUtc;
        Estado = EstadoCorreo.Enviado;
    }

    public void RegistrarFallo(DateTime ahoraUtc, string error)
    {
        ReservadoHastaUtc = null;
        Intentos++;
        UltimoIntentoEnUtc = ahoraUtc;
        UltimoError = error.Length > LargoMaximoError ? error.Substring(0, LargoMaximoError) : error;
        Estado = Intentos >= MaximoIntentos ? EstadoCorreo.Fallido : EstadoCorreo.Pendiente;
    }
}
