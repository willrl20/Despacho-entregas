using Despacho.Core.Comun;
using Despacho.Core.Datos;

namespace Despacho.Core.Correos;

public sealed class ColaCorreos : IColaCorreos
{
    private readonly CoreDbContext _db;
    private readonly IReloj _reloj;

    public ColaCorreos(CoreDbContext db, IReloj reloj)
    {
        _db = db;
        _reloj = reloj;
    }

    public void Encolar(string destinatario, string asunto, string cuerpo)
    {
        _db.CorreosEnCola.Add(new CorreoEnCola
        {
            Destinatario = destinatario,
            Asunto = asunto,
            Cuerpo = cuerpo,
            CreadoEnUtc = _reloj.AhoraUtc
        });
    }
}