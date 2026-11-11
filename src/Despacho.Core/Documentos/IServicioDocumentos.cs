using Despacho.Core.Comun;

namespace Despacho.Core.Documentos;

public interface IServicioDocumentos
{
    Task<Resultado<int>> SubirAsync(int usuarioId, string? nombre, string? tipoContenido, long tamanoBytes, Stream? contenido);
    Task<IReadOnlyList<DocumentoResumen>> ListarAsync(int usuarioId, bool esAdministrador);
    Task<Resultado<ArchivoDescarga>> DescargarAsync(int usuarioId, bool esAdministrador, int documentoId);
    Task<Resultado> EliminarAsync(int usuarioId, bool esAdministrador, int documentoId);
}
