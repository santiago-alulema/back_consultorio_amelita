using ClinicaAmelita.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAmelita.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuariosRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<HistoriaClinica> HistoriasClinicas => Set<HistoriaClinica>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Enfermedad> Enfermedades => Set<Enfermedad>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<RecetaDetalle> RecetasDetalles => Set<RecetaDetalle>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<ConteoCaja> ConteosCaja => Set<ConteoCaja>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<LoteProducto> LotesProductos => Set<LoteProducto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidosDetalles => Set<PedidoDetalle>();
    public DbSet<RecepcionPedido> RecepcionesPedidos => Set<RecepcionPedido>();
    public DbSet<RecepcionPedidoDetalle> RecepcionesPedidosDetalles => Set<RecepcionPedidoDetalle>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentasDetalles => Set<VentaDetalle>();
    public DbSet<DocumentoSri> DocumentosSri => Set<DocumentoSri>();
    public DbSet<DocumentoSriDetalle> DocumentosSriDetalles => Set<DocumentoSriDetalle>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("clinica");

        modelBuilder.Entity<Enfermedad>(entity =>
        {
            entity.HasKey(x => x.Codigo);
            entity.Property(x => x.Codigo).HasMaxLength(10);
            entity.Property(x => x.Nombre).HasMaxLength(1000);
            entity.Property(x => x.CategoriaCodigo).HasMaxLength(3);
            entity.Property(x => x.Categoria).HasMaxLength(1000);
            entity.HasIndex(x => x.CategoriaCodigo);
        });

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Paciente>()
            .HasIndex(x => x.Identificacion)
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasIndex(x => x.Codigo)
            .IsUnique();

        modelBuilder.Entity<DocumentoSri>()
            .HasIndex(x => x.ClaveAcceso)
            .IsUnique();

        modelBuilder.Entity<Caja>()
            .HasIndex(x => x.Fecha)
            .IsUnique();

        modelBuilder.Entity<Cita>()
            .HasOne(x => x.Consulta)
            .WithOne()
            .HasForeignKey<Cita>(x => x.ConsultaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UsuarioRol>()
            .HasKey(x => new { x.UsuarioId, x.RolId });

        modelBuilder.Entity<RolPermiso>()
            .HasKey(x => new { x.RolId, x.PermisoId });

        modelBuilder.Entity<Producto>()
            .Property(x => x.PrecioCompra)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Producto>()
            .Property(x => x.PrecioVenta)
            .HasPrecision(18, 4);

        modelBuilder.Entity<LoteProducto>()
            .Property(x => x.StockActual)
            .HasPrecision(18, 3);

        modelBuilder.Entity<MovimientoInventario>()
            .Property(x => x.Cantidad)
            .HasPrecision(18, 3);

        modelBuilder.Entity<Venta>()
            .Property(x => x.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Consulta>()
            .Property(x => x.ValorConsulta)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(x => x.MontoApertura)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(x => x.MontoEsperado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(x => x.MontoContado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(x => x.Diferencia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoCaja>()
            .Property(x => x.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConteoCaja>()
            .Property(x => x.Denominacion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConteoCaja>()
            .Ignore(x => x.Total);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaActualizacion = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
