using Despacho.Core.Comun;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.EnviadorCorreos;
using Microsoft.EntityFrameworkCore;

var conexion = Environment.GetEnvironmentVariable("DESPACHO_CONEXION");
var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
var smtpPuertoStr = Environment.GetEnvironmentVariable("SMTP_PUERTO");
var smtpUsuario = Environment.GetEnvironmentVariable("SMTP_USUARIO");
var smtpContrasena = Environment.GetEnvironmentVariable("SMTP_CONTRASENA");
var smtpRemitente = Environment.GetEnvironmentVariable("SMTP_REMITENTE");

if (string.IsNullOrEmpty(conexion))
{
    Console.WriteLine("Falta configurar la variable de entorno DESPACHO_CONEXION.");
    return 1;
}

if (string.IsNullOrEmpty(smtpHost))
{
    Console.WriteLine("Falta configurar la variable de entorno SMTP_HOST.");
    return 1;
}

if (string.IsNullOrEmpty(smtpPuertoStr) || !int.TryParse(smtpPuertoStr, out var smtpPuerto))
{
    Console.WriteLine("Falta configurar la variable de entorno SMTP_PUERTO.");
    return 1;
}

if (string.IsNullOrEmpty(smtpUsuario))
{
    Console.WriteLine("Falta configurar la variable de entorno SMTP_USUARIO.");
    return 1;
}

if (string.IsNullOrEmpty(smtpContrasena))
{
    Console.WriteLine("Falta configurar la variable de entorno SMTP_CONTRASENA.");
    return 1;
}

if (string.IsNullOrEmpty(smtpRemitente))
{
    Console.WriteLine("Falta configurar la variable de entorno SMTP_REMITENTE.");
    return 1;
}

var opciones = new DbContextOptionsBuilder<CoreDbContext>().UseSqlServer(conexion).Options;
using var db = new CoreDbContext(opciones);

var procesador = new ProcesadorCola(
    db,
    new RemitenteSmtp(new OpcionesSmtp
    {
        Host = smtpHost,
        Puerto = smtpPuerto,
        Usuario = smtpUsuario,
        Contrasena = smtpContrasena,
        Remitente = smtpRemitente
    }),
    new RelojSistema());

var resumen = await procesador.ProcesarPendientesAsync();
Console.WriteLine($"Correos enviados: {resumen.Enviados}. Fallidos: {resumen.Fallidos}.");
return 0;
