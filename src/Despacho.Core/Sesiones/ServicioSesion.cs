using Despacho.Core.Comun;
using Despacho.Core.Datos;
using Despacho.Core.Seguridad;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Sesiones;

public sealed class ServicioSesion : IServicioSesion
{
    private readonly CoreDbContext _db;
    private readonly IHasherContrasenas _hasher;
    private readonly IGeneradorTokens _generador;
    private readonly IReloj _reloj;

    private const int MaximoIntentos = 5;
    private const int MinutosBloqueo = 15;
    private const int HorasSesion = 8;
    private const string MensajeCredenciales = "Correo o contraseña incorrectos.";
    private const string MensajeBloqueado = "La cuenta está bloqueada por intentos fallidos. Intenta en 15 minutos.";
    private const string MensajeNoActiva = "La cuenta no está activa. Revisa tu correo para activarla.";
    private const string MensajeDesactivada = "La cuenta está desactivada.";

    public ServicioSesion(CoreDbContext db, IHasherContrasenas hasher, IGeneradorTokens generador, IReloj reloj)
    {
        _db = db;
        _hasher = hasher;
        _generador = generador;
        _reloj = reloj;
    }

    public async Task<Resultado<string>> IniciarAsync(string? correo, string? contrasena)
    {
        var correoNormalizado = NormalizadorCorreo.Normalizar(correo);
        if (correoNormalizado is null || string.IsNullOrEmpty(contrasena))
            return Resultado<string>.Falla(MensajeCredenciales);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNormalizado);
        if (usuario is null)
            return Resultado<string>.Falla(MensajeCredenciales);

        var ahora = _reloj.AhoraUtc;
        if (usuario.BloqueadoHastaUtc.HasValue && usuario.BloqueadoHastaUtc.Value > ahora)
            return Resultado<string>.Falla(MensajeBloqueado);

        if (!_hasher.Verificar(usuario.HashContrasena, contrasena))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= MaximoIntentos)
            {
                usuario.BloqueadoHastaUtc = ahora.AddMinutes(MinutosBloqueo);
                usuario.IntentosFallidos = 0;
            }
            await _db.SaveChangesAsync();
            return Resultado<string>.Falla(MensajeCredenciales);
        }

        if (!usuario.Activada)
            return Resultado<string>.Falla(MensajeNoActiva);

        if (usuario.Desactivado)
            return Resultado<string>.Falla(MensajeDesactivada);

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;

        var token = _generador.Generar();
        _db.Sesiones.Add(new Sesion
        {
            UsuarioId = usuario.Id,
            Token = token,
            CreadaEnUtc = ahora,
            VenceEnUtc = ahora.AddHours(HorasSesion)
        });

        await _db.SaveChangesAsync();
        return Resultado<string>.Ok(token);
    }

    public async Task<UsuarioActual?> ObtenerUsuarioAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var sesion = await _db.Sesiones.FirstOrDefaultAsync(s => s.Token == token);
        if (sesion is null || sesion.Revocada || sesion.VenceEnUtc <= _reloj.AhoraUtc)
            return null;

        var usuario = await _db.Usuarios.FindAsync(sesion.UsuarioId);
        if (usuario is null || !usuario.Activada || usuario.Desactivado)
            return null;

        return new UsuarioActual(usuario.Id, usuario.Correo, usuario.Rol);
    }

    public async Task CerrarAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        var sesion = await _db.Sesiones.FirstOrDefaultAsync(s => s.Token == token);
        if (sesion is not null && !sesion.Revocada)
        {
            sesion.Revocada = true;
            await _db.SaveChangesAsync();
        }
    }
}
