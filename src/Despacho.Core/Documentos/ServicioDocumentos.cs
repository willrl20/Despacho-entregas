using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Documentos;

public sealed class ServicioDocumentos : IServicioDocumentos
{
    private readonly CoreDbContext _db;
    private readonly IAlmacenArchivos _almacen;
    private readonly IReloj _reloj;

    public const string MensajeArchivoInvalido = "Selecciona un archivo que no esté vacío.";
    public const string MensajeArchivoGrande = "El archivo no puede pasar de 10 MB.";
    public const string MensajeNoExiste = "El documento no existe.";
    public const string MensajeSinPermiso = "No tienes permiso para este documento.";

    private const long TamanoMaximo = 10 * 1024 * 1024;

    public ServicioDocumentos(CoreDbContext db, IAlmacenArchivos almacen, IReloj reloj)
    {
        _db = db;
        _almacen = almacen;
        _reloj = reloj;
    }

    public async Task<Resultado<int>> SubirAsync(int usuarioId, string? nombre, string? tipoContenido, long tamanoBytes, Stream? contenido)
    {
        if (contenido == null || tamanoBytes <= 0)
            return Resultado<int>.Falla(MensajeArchivoInvalido);

        if (tamanoBytes > TamanoMaximo)
            return Resultado<int>.Falla(MensajeArchivoGrande);

        var nombreArchivo = Path.GetFileName(nombre ?? "").Trim();
        if (string.IsNullOrEmpty(nombreArchivo))
            nombreArchivo = "documento";
        if (nombreArchivo.Length > 255)
            nombreArchivo = nombreArchivo.Substring(0, 255);

        var tipo = string.IsNullOrWhiteSpace(tipoContenido) ? "application/octet-stream" : tipoContenido;
        if (tipo.Length > 100)
            tipo = tipo.Substring(0, 100);

        var nombreAlmacenado = Guid.NewGuid().ToString("N");

        await _almacen.GuardarAsync(nombreAlmacenado, contenido);

        var documento = new Documento
        {
            Nombre = nombreArchivo,
            TipoContenido = tipo,
            TamanoBytes = tamanoBytes,
            PropietarioId = usuarioId,
            SubidoEnUtc = _reloj.AhoraUtc,
            NombreAlmacenado = nombreAlmacenado,
            Eliminado = false
        };

        _db.Documentos.Add(documento);
        await _db.SaveChangesAsync();

        return Resultado<int>.Ok(documento.Id);
    }

    public async Task<IReadOnlyList<DocumentoResumen>> ListarAsync(int usuarioId, bool esAdministrador)
    {
        var query = _db.Documentos.Where(d => !d.Eliminado);

        if (!esAdministrador)
            query = query.Where(d => d.PropietarioId == usuarioId);

        return await query
            .OrderByDescending(d => d.SubidoEnUtc)
            .Select(d => new DocumentoResumen(d.Id, d.Nombre, d.TipoContenido, d.TamanoBytes, d.SubidoEnUtc, d.PropietarioId))
            .ToListAsync();
    }

    public async Task<Resultado<ArchivoDescarga>> DescargarAsync(int usuarioId, bool esAdministrador, int documentoId)
    {
        var documento = await _db.Documentos.FirstOrDefaultAsync(d => d.Id == documentoId);

        if (documento == null || documento.Eliminado)
            return Resultado<ArchivoDescarga>.Falla(MensajeNoExiste);

        if (!esAdministrador && documento.PropietarioId != usuarioId)
            return Resultado<ArchivoDescarga>.Falla(MensajeSinPermiso);

        var stream = _almacen.Abrir(documento.NombreAlmacenado);
        if (stream == null)
            return Resultado<ArchivoDescarga>.Falla(MensajeNoExiste);

        return Resultado<ArchivoDescarga>.Ok(new ArchivoDescarga(stream, documento.Nombre, documento.TipoContenido));
    }

    public async Task<Resultado> EliminarAsync(int usuarioId, bool esAdministrador, int documentoId)
    {
        var documento = await _db.Documentos.FirstOrDefaultAsync(d => d.Id == documentoId);

        if (documento == null || documento.Eliminado)
            return Resultado.Falla(MensajeNoExiste);

        if (!esAdministrador && documento.PropietarioId != usuarioId)
            return Resultado.Falla(MensajeSinPermiso);

        documento.Eliminado = true;
        documento.EliminadoEnUtc = _reloj.AhoraUtc;
        await _db.SaveChangesAsync();

        return Resultado.Ok();
    }
}
