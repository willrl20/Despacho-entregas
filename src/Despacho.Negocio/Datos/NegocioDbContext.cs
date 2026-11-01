using Despacho.Negocio.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace Despacho.Negocio.Datos;

public class NegocioDbContext : DbContext
{
    public NegocioDbContext(DbContextOptions<NegocioDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<LineaPedido> LineasPedido => Set<LineaPedido>();

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

            entity.Property(p => p.DestinoReferencia)
                .HasMaxLength(200);

            entity.HasIndex(p => p.RepartidorId);

            entity.HasMany(p => p.Lineas)
                .WithOne()
                .HasForeignKey(l => l.PedidoId);
        });

        modelBuilder.Entity<LineaPedido>(entity =>
        {
            entity.Property(l => l.Producto)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(l => l.PrecioUnitario)
                .HasPrecision(10, 2);
        });
    }
}
