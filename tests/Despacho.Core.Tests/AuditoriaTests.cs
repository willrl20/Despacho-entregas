using Despacho.Core.Administracion;
using Despacho.Core.Auditoria;
using Despacho.Core.Contrasenas;
using Despacho.Core.Correos;
using Despacho.Core.Datos;
using Despacho.Core.Documentos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Seguridad;
using Despacho.Core.Tests.Dobles;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Tests;

public class AuditoriaTests
{
    private const int AdministradorId = 1;
    private const int UsuarioId = 2;
    private const string ContrasenaNueva = "Nueva1234";

    private sealed class AlmacenEnMemoria : IAlmacenArchivos
    {
        public Task GuardarAsync(string nombreAlmacenado, Stream contenido) => Task.CompletedTask;

        public Stream? Abrir(string nombreAlmacenado) => null;
    }

    private static DbContextOptions<CoreDbContext> NuevaBase()
    {
        return new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static async Task SembrarUsuarios(CoreDbContext db)
    {
        db.Usuarios.Add(new Usuario { Id = AdministradorId, Correo = "a1@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Administrador });
        db.Usuarios.Add(new Usuario { Id = UsuarioId, Correo = "r@test.com", HashContrasena = "hash", Activada = true, Rol = Rol.Estandar });
        await db.SaveChangesAsync();
    }

    private static ServicioContrasenas CrearContrasenas(CoreDbContext db, RelojFijo reloj)
    {
        return new ServicioContrasenas(db, new ValidadorContrasena(), new HasherContrasenas(), reloj, new ColaCorreos(db, reloj));
    }

    private static void VerificarRegistro(RegistroAuditoria registro, int usuarioId, string accion, string entidad, int entidadId, string anterior, string nuevo)
    {
        Assert.True(registro.Id > 0);
        Assert.Equal(usuarioId, registro.UsuarioId);
        Assert.Equal(accion, registro.Accion);
        Assert.Equal(entidad, registro.Entidad);
        Assert.Equal(entidadId, registro.EntidadId);
        Assert.Equal(RelojFijo.Ahora, registro.FechaUtc);
        Assert.Equal(anterior, registro.ValorAnterior);
        Assert.Equal(nuevo, registro.ValorNuevo);
    }

    private static async Task<SolicitudPermiso> AgregarSolicitudPendiente(CoreDbContext db)
    {
        var solicitud = new SolicitudPermiso { SolicitanteId = UsuarioId, PermisoSolicitado = Permiso.Repartidor, CreadaEnUtc = RelojFijo.Ahora.AddDays(-1) };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();
        return solicitud;
    }

    private static ServicioPermisos CrearPermisos(CoreDbContext db, RelojFijo reloj)
    {
        return new ServicioPermisos(db, reloj, new ServicioNotificaciones(db, new ColaCorreos(db, reloj), reloj));
    }

    // A-01 RF-AUD-02
    [Fact]
    public async Task AprobarAsync_SolicitudPendiente_RegistraLaAprobacion()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var solicitud = await AgregarSolicitudPendiente(db);
        var servicio = CrearPermisos(db, new RelojFijo());

        // Act
        var resultado = await servicio.AprobarAsync(AdministradorId, solicitud.Id);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, AdministradorId, AccionesAuditoria.AprobarSolicitud, EntidadesAuditoria.SolicitudPermiso, solicitud.Id, "Pendiente", "Aplicada");
    }

    // A-02 RF-AUD-02
    [Fact]
    public async Task RechazarAsync_ConMotivo_RegistraElRechazoConElMotivo()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var solicitud = await AgregarSolicitudPendiente(db);
        var servicio = CrearPermisos(db, new RelojFijo());

        // Act
        var resultado = await servicio.RechazarAsync(AdministradorId, solicitud.Id, "Falta la licencia");

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, AdministradorId, AccionesAuditoria.RechazarSolicitud, EntidadesAuditoria.SolicitudPermiso, solicitud.Id, "Pendiente", "Rechazada: Falta la licencia");
    }

    // A-03 RF-AUD-03
    [Fact]
    public async Task SubirAsync_DocumentoValido_RegistraLaSubida()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = new ServicioDocumentos(db, new AlmacenEnMemoria(), new RelojFijo());
        using var contenido = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var resultado = await servicio.SubirAsync(UsuarioId, "licencia.pdf", "application/pdf", 3, contenido);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, UsuarioId, AccionesAuditoria.SubirDocumento, EntidadesAuditoria.Documento, resultado.Valor, "(no existía)", "licencia.pdf");
    }

    // A-04 RF-AUD-03
    [Fact]
    public async Task EliminarAsync_DocumentoPropio_RegistraLaEliminacion()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = new ServicioDocumentos(db, new AlmacenEnMemoria(), new RelojFijo());
        using var contenido = new MemoryStream(new byte[] { 1, 2, 3 });
        var subido = await servicio.SubirAsync(UsuarioId, "licencia.pdf", "application/pdf", 3, contenido);

        // Act
        var resultado = await servicio.EliminarAsync(UsuarioId, false, subido.Valor);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync(r => r.Accion == AccionesAuditoria.EliminarDocumento);
        VerificarRegistro(registro, UsuarioId, AccionesAuditoria.EliminarDocumento, EntidadesAuditoria.Documento, subido.Valor, "licencia.pdf", "(eliminado)");
    }

    // A-05 RF-AUD-04
    [Fact]
    public async Task CambiarRolAsync_EstandarAAdministrador_RegistraElAntesYElDespues()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = new ServicioAdministracion(db, new RelojFijo());

        // Act
        var resultado = await servicio.CambiarRolAsync(AdministradorId, UsuarioId, "Administrador");

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, AdministradorId, AccionesAuditoria.CambiarRol, EntidadesAuditoria.Usuario, UsuarioId, "Estandar", "Administrador");
    }

    // A-06 RF-AUD-04
    [Fact]
    public async Task DesactivarAsync_UsuarioActivo_RegistraLaDesactivacion()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = new ServicioAdministracion(db, new RelojFijo());

        // Act
        var resultado = await servicio.DesactivarAsync(AdministradorId, UsuarioId);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, AdministradorId, AccionesAuditoria.DesactivarUsuario, EntidadesAuditoria.Usuario, UsuarioId, "Activo", "Desactivado");
    }

    // A-06 RF-AUD-04
    [Fact]
    public async Task ReactivarAsync_UsuarioDesactivado_RegistraLaReactivacion()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = new ServicioAdministracion(db, new RelojFijo());
        await servicio.DesactivarAsync(AdministradorId, UsuarioId);

        // Act
        var resultado = await servicio.ReactivarAsync(AdministradorId, UsuarioId);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync(r => r.Accion == AccionesAuditoria.ReactivarUsuario);
        VerificarRegistro(registro, AdministradorId, AccionesAuditoria.ReactivarUsuario, EntidadesAuditoria.Usuario, UsuarioId, "Desactivado", "Activo");
    }

    // A-07 RF-AUD-05
    [Fact]
    public async Task RestablecerAsync_IniciadoPorElPropioUsuario_RegistraElOrigenPropio()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = CrearContrasenas(db, new RelojFijo());
        await servicio.SolicitarRecuperacionAsync("r@test.com");
        var codigo = (await db.CodigosRecuperacion.SingleAsync()).Codigo;

        // Act
        var resultado = await servicio.RestablecerAsync("r@test.com", codigo, ContrasenaNueva);

        // Assert
        Assert.True(resultado.Exito);
        var registro = await db.RegistrosAuditoria.SingleAsync();
        VerificarRegistro(registro, UsuarioId, AccionesAuditoria.RestablecerContrasena, EntidadesAuditoria.Usuario, UsuarioId, "Contraseña anterior", "Contraseña restablecida (iniciado por el propio usuario)");
    }

    // A-08 RF-AUD-05
    [Fact]
    public async Task RestablecerAsync_ForzadoPorAdministrador_RegistraLosDosEventos()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = CrearContrasenas(db, new RelojFijo());
        await servicio.ForzarRestablecimientoAsync(AdministradorId, UsuarioId);
        var codigo = (await db.CodigosRecuperacion.SingleAsync()).Codigo;

        // Act
        var resultado = await servicio.RestablecerAsync("r@test.com", codigo, ContrasenaNueva);

        // Assert
        Assert.True(resultado.Exito);
        var registros = await db.RegistrosAuditoria.OrderBy(r => r.Id).ToListAsync();
        Assert.Equal(2, registros.Count);
        VerificarRegistro(registros[0], AdministradorId, AccionesAuditoria.ForzarRestablecimiento, EntidadesAuditoria.Usuario, UsuarioId, "Contraseña vigente", "Código de restablecimiento enviado");
        VerificarRegistro(registros[1], UsuarioId, AccionesAuditoria.RestablecerContrasena, EntidadesAuditoria.Usuario, UsuarioId, "Contraseña anterior", "Contraseña restablecida (iniciado por un Administrador)");
    }

    // A-09 RF-AUD-05
    [Fact]
    public async Task RestablecerAsync_ForzadoPorAdministrador_NingunRegistroGuardaContrasenaHashNiCodigo()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        await SembrarUsuarios(db);
        var servicio = CrearContrasenas(db, new RelojFijo());
        await servicio.ForzarRestablecimientoAsync(AdministradorId, UsuarioId);
        var codigo = (await db.CodigosRecuperacion.SingleAsync()).Codigo;
        await servicio.RestablecerAsync("r@test.com", codigo, ContrasenaNueva);
        var hash = (await db.Usuarios.SingleAsync(u => u.Id == UsuarioId)).HashContrasena;

        // Act
        var registros = await db.RegistrosAuditoria.ToListAsync();
        var texto = string.Join("|", registros.SelectMany(r => new[] { r.Accion, r.Entidad, r.ValorAnterior, r.ValorNuevo }));

        // Assert
        Assert.NotEmpty(registros);
        Assert.DoesNotContain(ContrasenaNueva, texto);
        Assert.DoesNotContain(hash, texto);
        Assert.DoesNotContain(codigo, texto);
    }

    // A-12 RF-AUD-06
    [Fact]
    public async Task SaveChangesAsync_ModificarUnRegistroDeAuditoria_LoRechazaYNoGuarda()
    {
        // Arrange
        var opciones = NuevaBase();
        using var db = new CoreDbContext(opciones);
        db.RegistrarAuditoria(new RelojFijo(), AdministradorId, AccionesAuditoria.CambiarRol, EntidadesAuditoria.Usuario, UsuarioId, "Estandar", "Administrador");
        await db.SaveChangesAsync();
        var registro = await db.RegistrosAuditoria.SingleAsync();
        registro.ValorNuevo = "Alterado";

        // Act
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());

        // Assert
        Assert.Equal(CoreDbContext.MensajeAuditoriaInmutable, excepcion.Message);
        using var otra = new CoreDbContext(opciones);
        Assert.Equal("Administrador", (await otra.RegistrosAuditoria.SingleAsync()).ValorNuevo);
    }

    // A-12 RF-AUD-06
    [Fact]
    public async Task SaveChangesAsync_BorrarUnRegistroDeAuditoria_LoRechazaYNoBorra()
    {
        // Arrange
        var opciones = NuevaBase();
        using var db = new CoreDbContext(opciones);
        db.RegistrarAuditoria(new RelojFijo(), AdministradorId, AccionesAuditoria.CambiarRol, EntidadesAuditoria.Usuario, UsuarioId, "Estandar", "Administrador");
        await db.SaveChangesAsync();
        db.RegistrosAuditoria.Remove(await db.RegistrosAuditoria.SingleAsync());

        // Act
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());

        // Assert
        Assert.Equal(CoreDbContext.MensajeAuditoriaInmutable, excepcion.Message);
        using var otra = new CoreDbContext(opciones);
        Assert.Single(await otra.RegistrosAuditoria.ToListAsync());
    }

    // A-12 RF-AUD-06
    [Fact]
    public void SaveChanges_ModificarUnRegistroDeAuditoria_LoRechaza()
    {
        // Arrange
        using var db = new CoreDbContext(NuevaBase());
        db.RegistrarAuditoria(new RelojFijo(), AdministradorId, AccionesAuditoria.CambiarRol, EntidadesAuditoria.Usuario, UsuarioId, "Estandar", "Administrador");
        db.SaveChanges();
        db.RegistrosAuditoria.Single().ValorNuevo = "Alterado";

        // Act
        var excepcion = Assert.Throws<InvalidOperationException>(() => db.SaveChanges());

        // Assert
        Assert.Equal(CoreDbContext.MensajeAuditoriaInmutable, excepcion.Message);
    }
}
