using Despacho.Core.Permisos;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;

namespace Despacho.Core.Tests;

public class SolicitudPermisoTests
{
    // RF-GP-03
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

    // RF-GP-06
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

    // RF-GP-04
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

    // RF-GP-06
    [Theory]
    [InlineData(EstadoSolicitud.Pendiente, EstadoSolicitud.Pendiente)]
    [InlineData(EstadoSolicitud.Pendiente, EstadoSolicitud.Aplicada)]
    [InlineData(EstadoSolicitud.Aprobada, EstadoSolicitud.Pendiente)]
    [InlineData(EstadoSolicitud.Aprobada, EstadoSolicitud.Aprobada)]
    [InlineData(EstadoSolicitud.Aprobada, EstadoSolicitud.Rechazada)]
    [InlineData(EstadoSolicitud.Aplicada, EstadoSolicitud.Pendiente)]
    [InlineData(EstadoSolicitud.Aplicada, EstadoSolicitud.Aprobada)]
    [InlineData(EstadoSolicitud.Aplicada, EstadoSolicitud.Aplicada)]
    [InlineData(EstadoSolicitud.Aplicada, EstadoSolicitud.Rechazada)]
    [InlineData(EstadoSolicitud.Rechazada, EstadoSolicitud.Pendiente)]
    [InlineData(EstadoSolicitud.Rechazada, EstadoSolicitud.Aprobada)]
    [InlineData(EstadoSolicitud.Rechazada, EstadoSolicitud.Aplicada)]
    [InlineData(EstadoSolicitud.Rechazada, EstadoSolicitud.Rechazada)]
    public void TransicionesSolicitud_FueraDeLaTabla_NoEstaPermitida(EstadoSolicitud desde, EstadoSolicitud hacia)
    {
        // Arrange
        // Act
        // Assert
        Assert.False(TransicionesSolicitud.EsPermitida(desde, hacia));
    }

    // RF-GP-06
    [Theory]
    [InlineData(EstadoSolicitud.Pendiente, EstadoSolicitud.Aprobada)]
    [InlineData(EstadoSolicitud.Pendiente, EstadoSolicitud.Rechazada)]
    [InlineData(EstadoSolicitud.Aprobada, EstadoSolicitud.Aplicada)]
    public void TransicionesSolicitud_DeLaTabla_EstaPermitida(EstadoSolicitud desde, EstadoSolicitud hacia)
    {
        // Arrange
        // Act
        // Assert
        Assert.True(TransicionesSolicitud.EsPermitida(desde, hacia));
    }
}
