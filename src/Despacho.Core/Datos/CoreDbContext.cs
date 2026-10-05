using Despacho.Core.Contrasenas;
using Despacho.Core.Correos;
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

            entity.HasIndex(c => c.Enviado);
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
    }
}
