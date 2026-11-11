namespace Despacho.Core.Documentos;

public interface IAlmacenArchivos
{
    Task GuardarAsync(string nombreAlmacenado, Stream contenido);
    Stream? Abrir(string nombreAlmacenado);
}
