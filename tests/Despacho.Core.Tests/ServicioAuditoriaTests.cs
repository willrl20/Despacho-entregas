using Despacho.Core.Auditoria;
using Despacho.Core.Datos;
using Despacho.Core.Reportes;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class ServicioAuditoriaTests
{
    private static readonly UsuarioActual Administrador = new(1, "a1@test.com", Rol.Administrador);
    private static readonly UsuarioActual Estandar = new(2, "r@test.com", Rol.Estandar);

    private static CoreDbContext CrearBase()
    {
        return new CoreDbContext(new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    private static RegistroAuditoria Registro(int usuarioId, string entidad, DateTime fechaUtc)
    {
        return new RegistroAuditoria
        {
            UsuarioId = usuarioId,
            Accion = AccionesAuditoria.CambiarRol,
            Entidad = entidad,
            EntidadId = 5,
            FechaUtc = fechaUtc,
            ValorAnterior = "antes",
            ValorNuevo = "despues"
        };
    }

    // A-13 RF-AUD-07
    [Fact]
    public async Task ConsultarAsync_FiltroPorUsuario_DevuelveSoloLosDeEseUsuarioDelMasNuevoAlMasViejo()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.RegistrosAuditoria.AddRange(
            Registro(1, EntidadesAuditoria.Usuario, dia),
            Registro(2, EntidadesAuditoria.Usuario, dia.AddHours(1)),
            Registro(2, EntidadesAuditoria.Documento, dia.AddHours(3)),
            Registro(2, EntidadesAuditoria.Usuario, dia.AddHours(2)));
        await db.SaveChangesAsync();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, "2", null, null, null);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(3, resultado.Valor!.Count);
        Assert.All(resultado.Valor, r => Assert.Equal(2, r.UsuarioId));
        Assert.Equal(new[] { dia.AddHours(3), dia.AddHours(2), dia.AddHours(1) }, resultado.Valor.Select(r => r.FechaUtc).ToArray());
    }

    // A-14 RF-AUD-07
    [Fact]
    public async Task ConsultarAsync_FiltroPorEntidad_DevuelveSoloEsaEntidad()
    {
        // Arrange
        using var db = CrearBase();
        var dia = new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);
        db.RegistrosAuditoria.AddRange(
            Registro(1, EntidadesAuditoria.SolicitudPermiso, dia),
            Registro(1, EntidadesAuditoria.Documento, dia),
            Registro(2, EntidadesAuditoria.Documento, dia),
            Registro(1, EntidadesAuditoria.Usuario, dia));
        await db.SaveChangesAsync();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, "Documento", null, null);

        // Assert
        Assert.True(resultado.Exito);
        Assert.Equal(2, resultado.Valor!.Count);
        Assert.All(resultado.Valor, r => Assert.Equal(EntidadesAuditoria.Documento, r.Entidad));
    }

    // A-15 RF-AUD-07, RD-11
    [Fact]
    public async Task ConsultarAsync_RegistroDelUltimoDiaALas2330_SaleConEseDia()
    {
        // Arrange
        using var db = CrearBase();
        db.RegistrosAuditoria.Add(Registro(1, EntidadesAuditoria.Usuario, new DateTime(2026, 11, 20, 23, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, null, "2026-11-20", "2026-11-20");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Single(resultado.Valor!);
    }

    // A-15 RF-AUD-07, RD-11
    [Fact]
    public async Task ConsultarAsync_RegistroDelDia20_NoSaleConElRangoDelDia21()
    {
        // Arrange
        using var db = CrearBase();
        db.RegistrosAuditoria.Add(Registro(1, EntidadesAuditoria.Usuario, new DateTime(2026, 11, 20, 23, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, null, "2026-11-21", "2026-11-21");

        // Assert
        Assert.True(resultado.Exito);
        Assert.Empty(resultado.Valor!);
    }

    // A-16 RF-AUD-07, RD-06
    [Fact]
    public async Task ConsultarAsync_UsuarioEstandar_FallaConMensajeDeSinPermiso()
    {
        // Arrange
        using var db = CrearBase();
        db.RegistrosAuditoria.Add(Registro(1, EntidadesAuditoria.Usuario, new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Estandar, null, null, null, null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioAuditoria.MensajeSinPermiso, resultado.Error);
    }

    // A-17 RD-07
    [Fact]
    public async Task ConsultarAsync_UsuarioIdQueNoEsNumero_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, "abc", null, null, null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioAuditoria.MensajeUsuarioInvalido, resultado.Error);
    }

    // A-17 RD-07
    [Fact]
    public async Task ConsultarAsync_EntidadQueNoExiste_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, "Envio", null, null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(ServicioAuditoria.MensajeEntidadInvalida, resultado.Error);
    }

    // A-17 RD-07
    [Fact]
    public async Task ConsultarAsync_FechaQueNoExiste_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, null, "2026-13-40", null);

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(RangoFechas.MensajeFechaInvalida, resultado.Error);
    }

    // A-17 RD-07
    [Fact]
    public async Task ConsultarAsync_DesdePosteriorAHasta_FallaConMensajeClaro()
    {
        // Arrange
        using var db = CrearBase();
        var servicio = new ServicioAuditoria(db);

        // Act
        var resultado = await servicio.ConsultarAsync(Administrador, null, null, "2026-11-20", "2026-11-10");

        // Assert
        Assert.False(resultado.Exito);
        Assert.Equal(RangoFechas.MensajeRangoInvertido, resultado.Error);
    }

    // A-12 RF-AUD-06
    [Fact]
    public void IServicioAuditoria_NoTieneMetodosParaEditarNiBorrar()
    {
        // Arrange
        var prefijosProhibidos = new[] { "Editar", "Modificar", "Actualizar", "Borrar", "Eliminar" };

        // Act
        var prohibidos = typeof(IServicioAuditoria)
            .GetMethods()
            .Where(m => prefijosProhibidos.Any(p => m.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .Select(m => m.Name)
            .ToList();

        // Assert
        Assert.Empty(prohibidos);
        Assert.Single(typeof(IServicioAuditoria).GetMethods());
    }
}
