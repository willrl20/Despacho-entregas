namespace Despacho.Core.Documentos;

public sealed record ArchivoDescarga(Stream Contenido, string Nombre, string TipoContenido);
