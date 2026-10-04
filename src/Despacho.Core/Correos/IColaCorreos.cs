namespace Despacho.Core.Correos;

public interface IColaCorreos
{
    void Encolar(string destinatario, string asunto, string cuerpo);
}