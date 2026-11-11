namespace Despacho.Core.Documentos;

public sealed class AlmacenArchivosLocal : IAlmacenArchivos
{
    private readonly string _carpeta;

    public AlmacenArchivosLocal(string carpeta)
    {
        _carpeta = carpeta;
        Directory.CreateDirectory(_carpeta);
    }

    public async Task GuardarAsync(string nombreAlmacenado, Stream contenido)
    {
        var ruta = Path.Combine(_carpeta, nombreAlmacenado);
        using var archivo = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write);
        await contenido.CopyToAsync(archivo);
    }

    public Stream? Abrir(string nombreAlmacenado)
    {
        var ruta = Path.Combine(_carpeta, nombreAlmacenado);
        if (!File.Exists(ruta))
            return null;
        return new FileStream(ruta, FileMode.Open, FileAccess.Read);
    }
}
