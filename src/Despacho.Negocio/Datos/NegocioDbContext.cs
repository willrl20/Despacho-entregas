using Despacho.Negocio.Envios;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Datos;

public class NegocioDbContext : DbContext
{
    public NegocioDbContext(DbContextOptions<NegocioDbContext> options) : base(options)
    {
    }

    public DbSet<Envio> Envios => Set<Envio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.ToTable("Envios");

            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Modalidad)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.Tamano)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(e => e.OrigenReferencia)
                .HasMaxLength(200);

            entity.Property(e => e.DestinoReferencia)
                .HasMaxLength(200);

            entity.Property(e => e.ContactoNombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ContactoTelefono)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DescripcionPaquete)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Costo)
                .HasPrecision(10, 2);

            entity.HasIndex(e => e.ClienteId);

            entity.HasIndex(e => e.RepartidorId);

            entity.HasIndex(e => e.Estado);
        });
    }
}
