using Despacho.Api.Endpoints;
using Despacho.Core.Administracion;
using Despacho.Core.Comun;
using Despacho.Core.Contrasenas;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration["DESPACHO_CONEXION"] ?? throw new InvalidOperationException("Falta la variable de entorno DESPACHO_CONEXION.");
var urlActivacion = builder.Configuration["DESPACHO_URL_ACTIVACION"] ?? throw new InvalidOperationException("Falta la variable de entorno DESPACHO_URL_ACTIVACION.");

builder.Services.AddDbContext<CoreDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));
builder.Services.AddSingleton<IReloj, RelojSistema>();
builder.Services.AddSingleton<IValidadorContrasena, ValidadorContrasena>();
builder.Services.AddSingleton<IHasherContrasenas, HasherContrasenas>();
builder.Services.AddSingleton<IGeneradorTokens, GeneradorTokens>();
builder.Services.AddSingleton(new OpcionesActivacion { UrlBase = urlActivacion });
builder.Services.AddScoped<IColaCorreos, ColaCorreos>();
builder.Services.AddScoped<IServicioRegistro, ServicioRegistro>();
builder.Services.AddScoped<IServicioSesion, ServicioSesion>();
builder.Services.AddScoped<IServicioAdministracion, ServicioAdministracion>();
builder.Services.AddScoped<IServicioContrasenas, ServicioContrasenas>();
builder.Services.Configure<RouteHandlerOptions>(opciones => opciones.ThrowOnBadRequest = false);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
app.Run();
