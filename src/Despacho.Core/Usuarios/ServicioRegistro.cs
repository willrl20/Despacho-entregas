using System;
using System.Threading.Tasks;
using Despacho.Core.Comun;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Usuarios;

public sealed class ServicioRegistro : IServicioRegistro
{
    private const string MensajeCorreoInvalido = "El correo no es válido.";
    private const string MensajeCorreoExistente = "Ya existe una cuenta con ese correo.";
    private const string MensajeEnlaceInvalido = "El enlace de activación no es válido o ya venció.";

    private readonly CoreDbContext _db;
    private readonly IValidadorContrasena _validador;
    private readonly IHasherContrasenas _hasher;
    private readonly IGeneradorTokens _generador;
    private readonly IReloj _reloj;
    private readonly OpcionesActivacion _opciones;

    public ServicioRegistro(
        CoreDbContext db,
        IValidadorContrasena validador,
        IHasherContrasenas hasher,
        IGeneradorTokens generador,
        IReloj reloj,
        OpcionesActivacion opciones)
    {
        _db = db;
        _validador = validador;
        _hasher = hasher;
        _generador = generador;
        _reloj = reloj;
        _opciones = opciones;
    }

    public async Task<Resultado> RegistrarAsync(string? correo, string? contrasena)
    {
        var correoNormalizado = NormalizarCorreo(correo);
        if (correoNormalizado is null)
        {
            return Resultado.Falla(MensajeCorreoInvalido);
        }

        var validacion = _validador.Validar(contrasena);
        if (!validacion.Exito)
        {
            return validacion;
        }

        if (await _db.Usuarios.AnyAsync(u => u.Correo == correoNormalizado))
        {
            return Resultado.Falla(MensajeCorreoExistente);
        }

        var usuario = new Usuario
        {
            Correo = correoNormalizado,
            HashContrasena = _hasher.Hashear(contrasena!),
            Activada = false,
            CreadoEnUtc = _reloj.AhoraUtc
        };

        _db.Usuarios.Add(usuario);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Falla(MensajeCorreoExistente);
        }

        AgregarTokenYCorreo(usuario);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> ActivarAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Resultado.Falla(MensajeEnlaceInvalido);
        }

        var registro = await _db.TokensActivacion.FirstOrDefaultAsync(t => t.Token == token);
        if (registro is null ||
            registro.Usado ||
            registro.Invalidado ||
            registro.VenceEnUtc <= _reloj.AhoraUtc)
        {
            return Resultado.Falla(MensajeEnlaceInvalido);
        }

        var usuario = await _db.Usuarios.FindAsync(registro.UsuarioId);
        if (usuario is null)
        {
            return Resultado.Falla(MensajeEnlaceInvalido);
        }

        registro.Usado = true;
        usuario.Activada = true;
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    public async Task<Resultado> ReenviarActivacionAsync(string? correo)
    {
        var correoNormalizado = NormalizarCorreo(correo);
        if (correoNormalizado is null)
        {
            return Resultado.Ok();
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNormalizado);
        if (usuario is null || usuario.Activada)
        {
            return Resultado.Ok();
        }

        var tokens = await _db.TokensActivacion
            .Where(t => t.UsuarioId == usuario.Id && !t.Usado && !t.Invalidado)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Invalidado = true;
        }

        AgregarTokenYCorreo(usuario);
        await _db.SaveChangesAsync();
        return Resultado.Ok();
    }

    private void AgregarTokenYCorreo(Usuario usuario)
    {
        var token = _generador.Generar();

        _db.TokensActivacion.Add(new TokenActivacion
        {
            UsuarioId = usuario.Id,
            Token = token,
            VenceEnUtc = _reloj.AhoraUtc + _opciones.Vigencia
        });

        _db.CorreosEnCola.Add(new CorreoEnCola
        {
            Destinatario = usuario.Correo,
            Asunto = "Activa tu cuenta",
            Cuerpo = $"Para activar tu cuenta abre este enlace: {_opciones.UrlBase}?token={token}",
            CreadoEnUtc = _reloj.AhoraUtc
        });
    }

    private static string? NormalizarCorreo(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return null;
        }

        var normalizado = correo.Trim().ToLowerInvariant();
        if (normalizado.Length > 256 || !System.Net.Mail.MailAddress.TryCreate(normalizado, out _))
        {
            return null;
        }

        return normalizado;
    }
}