using Despacho.Core.Comun;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Seguridad;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Contrasenas;

public sealed class ServicioContrasenas : IServicioContrasenas
{
    private readonly CoreDbContext _db;
    private readonly IValidadorContrasena _validador;
    private readonly IHasherContrasenas _hasher;
    private readonly IReloj _reloj;
    private readonly IColaCorreos _cola;

    private const int MinutosVigencia = 30;
    private const string MensajeCodigoInvalido = "El código no es válido o ya venció.";
    private const string MensajeNoExiste = "El usuario no existe.";
    private const string MensajeActualIncorrecta = "La contraseña actual no es correcta.";

    public ServicioContrasenas(CoreDbContext db, IValidadorContrasena validador, IHasherContrasenas hasher, IReloj reloj, IColaCorreos cola)
    {
        _db = db;
        _validador = validador;
        _hasher = hasher;
        _reloj = reloj;
        _cola = cola;
    }

    private void EmitirCodigo(Usuario usuario)
    {
        var pendientes = _db.CodigosRecuperacion
            .Where(c => c.UsuarioId == usuario.Id && !c.Usado && !c.Invalidado)
            .ToList();

        foreach (var pendiente in pendientes)
        {
            pendiente.Invalidado = true;
        }

        var codigo = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");

        _db.CodigosRecuperacion.Add(new CodigoRecuperacion
        {
            UsuarioId = usuario.Id,
            Codigo = codigo,
            VenceEnUtc = _reloj.AhoraUtc.AddMinutes(MinutosVigencia)
        });

        _cola.Encolar(usuario.Correo, "Código para restablecer tu contraseña", $"Tu código es {codigo}. Vence en {MinutosVigencia} minutos. Si no lo pediste, ignora este correo.");
    }

    private async Task RevocarSesionesAsync(int usuarioId)
    {
        var sesiones = await _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && !s.Revocada)
            .ToListAsync();

        foreach (var sesion in sesiones)
        {
            sesion.Revocada = true;
        }
    }

    public async Task<Resultado> SolicitarRecuperacionAsync(string? correo)
    {
        var c = NormalizadorCorreo.Normalizar(correo);
        if (c is null)
            return Resultado.Ok();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == c);
        if (usuario is null || !usuario.Activada || usuario.Desactivado)
            return Resultado.Ok();

        EmitirCodigo(usuario);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> RestablecerAsync(string? correo, string? codigo, string? contrasenaNueva)
    {
        var c = NormalizadorCorreo.Normalizar(correo);
        if (c is null || string.IsNullOrWhiteSpace(codigo))
            return Resultado.Falla(MensajeCodigoInvalido);

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == c);
        if (usuario is null)
            return Resultado.Falla(MensajeCodigoInvalido);

        var registro = await _db.CodigosRecuperacion.FirstOrDefaultAsync(r =>
            r.UsuarioId == usuario.Id &&
            r.Codigo == codigo.Trim() &&
            !r.Usado &&
            !r.Invalidado &&
            r.VenceEnUtc > _reloj.AhoraUtc);

        if (registro is null)
            return Resultado.Falla(MensajeCodigoInvalido);

        var validacion = _validador.Validar(contrasenaNueva);
        if (!validacion.Exito)
            return validacion;

        usuario.HashContrasena = _hasher.Hashear(contrasenaNueva!);
        registro.Usado = true;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHastaUtc = null;

        await RevocarSesionesAsync(usuario.Id);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> ForzarRestablecimientoAsync(int usuarioId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        EmitirCodigo(usuario);
        await RevocarSesionesAsync(usuario.Id);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> CambiarAsync(int usuarioId, string? contrasenaActual, string? contrasenaNueva)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null)
            return Resultado.Falla(MensajeNoExiste);

        if (string.IsNullOrEmpty(contrasenaActual) || !_hasher.Verificar(usuario.HashContrasena, contrasenaActual))
            return Resultado.Falla(MensajeActualIncorrecta);

        var validacion = _validador.Validar(contrasenaNueva);
        if (!validacion.Exito)
            return validacion;

        usuario.HashContrasena = _hasher.Hashear(contrasenaNueva!);
        await RevocarSesionesAsync(usuario.Id);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }
}
