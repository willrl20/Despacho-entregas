using Despacho.Api.Endpoints;
using Despacho.Core.Administracion;
using Despacho.Core.Comun;
using Despacho.Core.Contrasenas;
using Despacho.Core.Correos;
using Despacho.Core.Documentos;
using Despacho.Core.Permisos;
using Despacho.Core.Datos;
using Despacho.Core.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Despacho.Negocio.Datos;
using Despacho.Negocio.Direcciones;
using Despacho.Negocio.Pedidos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration["DESPACHO_CONEXION"] ?? throw new InvalidOperationException("Falta la variable de entorno DESPACHO_CONEXION.");
var urlActivacion = builder.Configuration["DESPACHO_URL_ACTIVACION"] ?? throw new InvalidOperationException("Falta la variable de entorno DESPACHO_URL_ACTIVACION.");
var rutaDocumentos = Environment.GetEnvironmentVariable("DESPACHO_RUTA_DOCUMENTOS") ?? Path.Combine(builder.Environment.ContentRootPath, "documentos");

builder.Services.AddDbContext<CoreDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));
builder.Services.AddDbContext<NegocioDbContext>(opciones => opciones.UseSqlServer(cadenaConexion, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Negocio")));
builder.Services.AddSingleton<IReloj, RelojSistema>();
builder.Services.AddSingleton<IValidadorContrasena, ValidadorContrasena>();
builder.Services.AddSingleton<IHasherContrasenas, HasherContrasenas>();
builder.Services.AddSingleton<IGeneradorTokens, GeneradorTokens>();
builder.Services.AddSingleton(new OpcionesActivacion { UrlBase = urlActivacion });
builder.Services.AddSingleton<IAlmacenArchivos>(new AlmacenArchivosLocal(rutaDocumentos));
builder.Services.AddSingleton(new OpcionesOpenCage { ClaveApi = builder.Configuration["OPENCAGE_API_KEY"] });
builder.Services.AddHttpClient<IGeocodificador, GeocodificadorOpenCage>(cliente => cliente.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IColaCorreos, ColaCorreos>();
builder.Services.AddScoped<IServicioRegistro, ServicioRegistro>();
builder.Services.AddScoped<IServicioSesion, ServicioSesion>();
builder.Services.AddScoped<IServicioAdministracion, ServicioAdministracion>();
builder.Services.AddScoped<IServicioContrasenas, ServicioContrasenas>();
builder.Services.AddScoped<IServicioPermisos, ServicioPermisos>();
builder.Services.AddScoped<IServicioPedidos, ServicioPedidos>();
builder.Services.AddScoped<IServicioDocumentos, ServicioDocumentos>();
builder.Services.AddScoped<IServicioDirecciones, ServicioDirecciones>();
builder.Services.Configure<RouteHandlerOptions>(opciones => opciones.ThrowOnBadRequest = false);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var alcanceMigraciones = app.Services.CreateScope())
{
    var core = alcanceMigraciones.ServiceProvider.GetRequiredService<CoreDbContext>();
    var negocio = alcanceMigraciones.ServiceProvider.GetRequiredService<NegocioDbContext>();
    for (var intento = 1; ; intento++)
    {
        try
        {
            await core.Database.MigrateAsync();
            await negocio.Database.MigrateAsync();
            break;
        }
        catch (Exception) when (intento < 20)
        {
            Console.WriteLine($"La base de datos no está lista (intento {intento}). Reintentando en 5 segundos...");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

using (var alcance = app.Services.CreateScope())
{
    var administracion = alcance.ServiceProvider.GetRequiredService<IServicioAdministracion>();
    await administracion.PromoverAdministradorInicialAsync(builder.Configuration["DESPACHO_ADMIN_CORREO"]);
}

app.UseExceptionHandler(errores => errores.Run(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new { error = "Ocurrió un error inesperado." });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapCuentas();
app.MapSesion();
app.MapUsuarios();
app.MapContrasenas();
app.MapSolicitudes();
app.MapPedidos();
app.MapDocumentos();
app.Run();
