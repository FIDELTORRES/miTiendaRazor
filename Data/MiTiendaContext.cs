using Microsoft.EntityFrameworkCore;
using miTienda.Models;  // ← Importante: tus modelos

namespace miTienda.Data
{
    public class MiTiendaContext : DbContext
    {
        public MiTiendaContext(DbContextOptions<MiTiendaContext> options)
            : base(options)
        {
        }

        // ============================================================
        // 📌 DBSETS EXISTENTES (los que ya tenías)
        // ============================================================
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoTienda> ProductosTienda { get; set; }
        public DbSet<Subcategoria> Subcategorias { get; set; }
        public DbSet<Categoriapropia> CategoriasPropias { get; set; }
        public DbSet<Categoriapagina> CategoriasPagina { get; set; }
        public DbSet<Subcategoriapagina> SubcategoriasPagina { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Medida> Medidas { get; set; }
        public DbSet<Kardex> Kardex { get; set; }
        public DbSet<Tienda> Tiendas { get; set; }

        // ============================================================
        // 📌 NUEVOS DBSETS PARA EL CARRITO Y PAGOS
        // ============================================================
        public DbSet<VentaTemporal> VentasTemporales { get; set; }
        public DbSet<PedidoWeb> PedidosWeb { get; set; }
        public DbSet<DetalleWeb> DetallesWeb { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<PagoMercadoPago> PagosMercadoPago { get; set; }

        // ============================================================
        // 📌 CONFIGURACIONES ADICIONALES (OnModelCreating)
        // ============================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para las nuevas tablas
            modelBuilder.Entity<VentaTemporal>(entity =>
            {
                entity.ToTable("venta_temporal");
                entity.HasKey(e => e.IdVentaTemporal);
                // No mapeamos SessionId porque es [NotMapped]
            });

            modelBuilder.Entity<PedidoWeb>(entity =>
            {
                entity.ToTable("pedidoweb");
                entity.HasKey(e => e.IdPedidoWeb);
            });

            modelBuilder.Entity<DetalleWeb>(entity =>
            {
                entity.ToTable("detalleweb");
                entity.HasKey(e => e.IdDetalleWeb);
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("pagos");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<PagoMercadoPago>(entity =>
            {
                entity.ToTable("pagos_mercadopago");
                entity.HasKey(e => e.Id);
            });

            // Aquí van las configuraciones de las tablas existentes si las tienes
            // (ej: producto, productotienda, etc.)
        }
    }
}