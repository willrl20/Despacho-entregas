using Despacho.Api.Seguridad;
using Despacho.Core.Sesiones;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Despacho.Core.Tests;

public class AccesoCorreosTests
{
    // C-18 RF-NOT-11
    [Fact]
    public async Task InvokeAsync_UsuarioEstandarEnRutaDeAdministrador_Responde403SinLlegarAlEndpoint()
    {
        // Arrange
        var servicios = new ServiceCollection()
            .AddSingleton<IServicioSesion>(new ServicioSesionDoble(new UsuarioActual(4, "repartidor@test.com", Rol.Estandar)))
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = servicios };
        http.Request.Headers.Authorization = "Bearer token-valido";
        var filtro = new FiltroRol(Rol.Administrador);
        var llegoAlEndpoint = false;

        // Act
        var respuesta = await filtro.InvokeAsync(new DefaultEndpointFilterInvocationContext(http), _ =>
        {
            llegoAlEndpoint = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        // Assert
        Assert.False(llegoAlEndpoint);
        var resultado = Assert.IsAssignableFrom<IStatusCodeHttpResult>(respuesta);
        Assert.Equal(StatusCodes.Status403Forbidden, resultado.StatusCode);
    }
}
