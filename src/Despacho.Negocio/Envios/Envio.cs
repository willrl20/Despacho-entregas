namespace Despacho.Negocio.Envios;

public class Envio
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int? RepartidorId { get; set; }
    public EstadoEnvio Estado { get; private set; } = EstadoEnvio.Pendiente;
    public DateTime CreadoEnUtc { get; set; }
    public ModalidadEnvio Modalidad { get; set; }
    public double OrigenLatitud { get; set; }
    public double OrigenLongitud { get; set; }
    public string? OrigenReferencia { get; set; }
    public double DestinoLatitud { get; set; }
    public double DestinoLongitud { get; set; }
    public string? DestinoReferencia { get; set; }
    public string ContactoNombre { get; set; } = string.Empty;
    public string ContactoTelefono { get; set; } = string.Empty;
    public string DescripcionPaquete { get; set; } = string.Empty;
    public TamanoPaquete Tamano { get; set; }
    public double DistanciaKm { get; set; }
    public decimal Costo { get; set; }

    public bool CambiarEstado(EstadoEnvio nuevo)
    {
        if (!MaquinaEstadosEnvio.PuedeCambiar(Estado, nuevo))
            return false;

        Estado = nuevo;
        return true;
    }
}
