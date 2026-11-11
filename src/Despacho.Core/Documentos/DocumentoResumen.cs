namespace Despacho.Core.Documentos;

public sealed record DocumentoResumen(int Id, string Nombre, string TipoContenido, long TamanoBytes, DateTime SubidoEnUtc, int PropietarioId);
