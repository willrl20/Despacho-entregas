using Despacho.Negocio.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Datos;

public class NegocioDbContext : DbContext
{
    public NegocioDbContext(DbContextOptions<NegocioDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.Property(p => p.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(p => p.TiendaId);

            entity.HasIndex(p => p.DestinatarioId);
        });
    }
}
