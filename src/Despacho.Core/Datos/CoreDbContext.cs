using Despacho.Core.Auditoria;
using Despacho.Core.Contrasenas;
using Despacho.Core.Correos;
using Despacho.Core.Documentos;
using Despacho.Core.Notificaciones;
using Despacho.Core.Permisos;
using Despacho.Core.Sesiones;
using Despacho.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Core.Datos;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TokenActivacion> TokensActivacion => Set<TokenActivacion>();
    public DbSet<CorreoEnCola> CorreosEnCola => Set<CorreoEnCola>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<CodigoRecuperacion> CodigosRecuperacion => Set<CodigoRecuperacion>();
    public DbSet<SolicitudPermiso> SolicitudesPermiso => Set<SolicitudPermiso>();
    public DbSet<PermisoConcedido> PermisosConcedidos => Set<PermisoConcedido>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion => Set<PreferenciaNotificacion>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(u => u.Correo)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(u => u.Correo)
                .IsUnique();

            entity.Property(u => u.HashContrasena)
                .IsRequired();

            entity.Property(u => u.Rol)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<TokenActivacion>(entity =>
        {
            entity.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(t => t.Token)
                .IsUnique();

            entity.HasIndex(t => t.UsuarioId);
        });

        modelBuilder.Entity<CorreoEnCola>(entity =>
        {
            entity.Property(c => c.Destinatario)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(c => c.Asunto)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.Cuerpo)
                .IsRequired();

            entity.Property(c => c.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(EstadoCorreo.Pendiente);

            entity.Property(c => c.UltimoError)
                .HasMaxLength(500);

            entity.Property(c => c.ReservadoHastaUtc)
                .IsConcurrencyToken();

            entity.HasIndex(c => c.Estado);
        });

        modelBuilder.Entity<Sesion>(entity =>
        {
            entity.Property(s => s.Token)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(s => s.Token)
                .IsUnique();

            entity.HasIndex(s => s.UsuarioId);
        });

        modelBuilder.Entity<CodigoRecuperacion>(entity =>
        {
            entity.Property(c => c.Codigo)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(c => c.UsuarioId);
        });

        modelBuilder.Entity<SolicitudPermiso>(entity =>
        {
            entity.Property(s => s.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(s => s.PermisoSolicitado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(s => s.MotivoRechazo)
                .HasMaxLength(500);

            entity.HasIndex(s => s.SolicitanteId);
            entity.HasIndex(s => s.Estado);
        });

        modelBuilder.Entity<PermisoConcedido>(entity =>
        {
            entity.Property(p => p.Permiso)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(p => new { p.UsuarioId, p.Permiso })
                .IsUnique();
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.Property(d => d.Nombre)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.TipoContenido)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.NombreAlmacenado)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(d => d.PropietarioId);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.Property(n => n.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(n => n.Mensaje)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasIndex(n => new { n.DestinatarioId, n.Leida });
        });

        modelBuilder.Entity<PreferenciaNotificacion>(entity =>
        {
            entity.HasIndex(p => p.UsuarioId)
                .IsUnique();
        });

        modelBuilder.Entity<RegistroAuditoria>(entity =>
        {
            entity.Property(r => r.Accion)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(r => r.Entidad)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(r => r.ValorAnterior)
                .IsRequired()
                .HasMaxLength(AuditoriaExtensiones.LargoMaximoValor);

            entity.Property(r => r.ValorNuevo)
                .IsRequired()
                .HasMaxLength(AuditoriaExtensiones.LargoMaximoValor);

            entity.Ignore(r => r.EntidadReferencia);

            entity.HasIndex(r => r.UsuarioId);
            entity.HasIndex(r => r.Entidad);
            entity.HasIndex(r => r.FechaUtc);
        });
    }

    public const string MensajeAuditoriaInmutable = "El historial de auditoría no se puede modificar ni borrar.";

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        VerificarInmutabilidad();
        var pendientes = SepararAuditoriasPendientes();
        if (pendientes.Count == 0)
            return base.SaveChanges(acceptAllChangesOnSuccess);

        using var transaccion = IniciarTransaccion();
        var guardados = base.SaveChanges(acceptAllChangesOnSuccess);
        CompletarAuditorias(pendientes);
        guardados += base.SaveChanges(acceptAllChangesOnSuccess);
        transaccion?.Commit();
        return guardados;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        VerificarInmutabilidad();
        var pendientes = SepararAuditoriasPendientes();
        if (pendientes.Count == 0)
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        await using var transaccion = await IniciarTransaccionAsync(cancellationToken);
        var guardados = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        CompletarAuditorias(pendientes);
        guardados += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (transaccion != null)
            await transaccion.CommitAsync(cancellationToken);
        return guardados;
    }

    private void VerificarInmutabilidad()
    {
        var hayCambios = ChangeTracker.Entries<RegistroAuditoria>()
            .Any(e => e.State == EntityState.Modified || e.State == EntityState.Deleted);

        if (hayCambios)
            throw new InvalidOperationException(MensajeAuditoriaInmutable);
    }

    private List<RegistroAuditoria> SepararAuditoriasPendientes()
    {
        var pendientes = ChangeTracker.Entries<RegistroAuditoria>()
            .Where(e => e.State == EntityState.Added && e.Entity.EntidadReferencia != null)
            .ToList();

        foreach (var entrada in pendientes)
            entrada.State = EntityState.Detached;

        return pendientes.Select(e => e.Entity).ToList();
    }

    private void CompletarAuditorias(List<RegistroAuditoria> pendientes)
    {
        foreach (var registro in pendientes)
        {
            registro.EntidadId = (int)Entry(registro.EntidadReferencia!).Property("Id").CurrentValue!;
            registro.EntidadReferencia = null;
            RegistrosAuditoria.Add(registro);
        }
    }

    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? IniciarTransaccion()
    {
        return Database.IsRelational() && Database.CurrentTransaction == null
            ? Database.BeginTransaction()
            : null;
    }

    private async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction?> IniciarTransaccionAsync(CancellationToken cancellationToken)
    {
        return Database.IsRelational() && Database.CurrentTransaction == null
            ? await Database.BeginTransactionAsync(cancellationToken)
            : null;
    }
}
