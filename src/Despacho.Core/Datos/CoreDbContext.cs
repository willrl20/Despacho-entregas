using Despacho.Core.Correos;
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
    }
}
