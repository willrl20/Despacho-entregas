using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;

namespace Despacho.Core.Tests;

public class SolicitudPermisoTests
{
    [Fact]
    public void Aprobar_SolicitudPendiente_RegistraAprobadorYFecha()
    {
        // Arrange
        var solicitud = new SolicitudPermiso
        {
            Id = 1,
            SolicitanteId = 10,
            PermisoSolicitado = Rol.Administrador,
            CreadaEnUtc = RelojFijo.Ahora.AddDays(-1)
        };
        var aprobadorId = 20;
        var fechaAprobacion = RelojFijo.Ahora;

        // Act
        var resultado = solicitud.Aprobar(aprobadorId, fechaAprobacion);

        // Assert
        Assert.True(resultado);
        Assert.Equal(EstadoSolicitud.Aprobada, solicitud.Estado);
        Assert.Equal(aprobadorId, solicitud.AprobadorId);
        Assert.Equal(fechaAprobacion, solicitud.ResueltaEnUtc);
    }

    [Fact]
    public void MarcarAplicada_SolicitudRechazada_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var solicitud = new SolicitudPermiso
        {
            Id = 1,
            SolicitanteId = 10,
            PermisoSolicitado = Rol.Administrador,
            CreadaEnUtc = RelojFijo.Ahora.AddDays(-1)
        };
        solicitud.Rechazar(20, "No cumple requisitos", RelojFijo.Ahora);

        // Act
        var resultado = solicitud.MarcarAplicada();

        // Assert
        Assert.False(resultado);
        Assert.Equal(EstadoSolicitud.Rechazada, solicitud.Estado);
    }

    [Fact]
    public void Rechazar_SinMotivo_SeRechazaYElEstadoNoCambia()
    {
        // Arrange
        var solicitud = new SolicitudPermiso
        {
            Id = 1,
            SolicitanteId = 10,
            PermisoSolicitado = Rol.Administrador,
            CreadaEnUtc = RelojFijo.Ahora.AddDays(-1)
        };

        // Act
        var resultado = solicitud.Rechazar(20, null, RelojFijo.Ahora);

        // Assert
        Assert.False(resultado);
        Assert.Equal(EstadoSolicitud.Pendiente, solicitud.Estado);
        Assert.Null(solicitud.MotivoRechazo);
    }
}
