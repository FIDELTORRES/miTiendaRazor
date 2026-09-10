using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using miTienda.Models;

namespace miTienda.Data
{
    public class MiTiendaContext : IdentityDbContext<UsuarioCliente, IdentityRole<int>, int>
    {
        public MiTiendaContext(DbContextOptions<MiTiendaContext> options)
            : base(options)
        {
        }

        // ============================================================
        // 📌 DBSETS EXISTENTES
        // ============================================================
        public DbSet<Cliente> Clientes { get; set; }
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
        // 📌 DBSETS PARA CARRITO Y PAGOS
        // ============================================================
        public DbSet<VentaTemporal> VentasTemporales { get; set; }
        public DbSet<PedidoWeb> PedidosWeb { get; set; }
        public DbSet<DetalleWeb> DetallesWeb { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<PagoMercadoPago> PagosMercadoPago { get; set; }
        public DbSet<HorarioEntrega> HorariosEntrega { get; set; }

        // ============================================================
        // 📌 CONFIGURACIONES (OnModelCreating)
        // ============================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // 🔧 CONFIGURACIÓN DE USUARIO_CLIENTE
            // ============================================================
            modelBuilder.Entity<UsuarioCliente>(entity =>
            {
                entity.ToTable("usuario_cliente");

                // ✅ Configurar propiedades de Identity
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.UserName).HasColumnName("user_name");
                entity.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
                entity.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
                entity.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
                entity.Property(u => u.PhoneNumber).HasColumnName("phone_number");
                entity.Property(u => u.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
                entity.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
                entity.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
                entity.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
                entity.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");

                // ✅ Configurar campos adicionales
                entity.Property(u => u.IdCliente).HasColumnName("idcliente");
                entity.Property(u => u.Nombres).HasColumnName("nombres");
                entity.Property(u => u.Apellidos).HasColumnName("apellidos");
                entity.Property(u => u.Telefono).HasColumnName("telefono");
                entity.Property(u => u.Direccion).HasColumnName("direccion");
                entity.Property(u => u.FechaRegistro).HasColumnName("fecha_registro");
                entity.Property(u => u.Estado).HasColumnName("estado");
                entity.Property(u => u.FechaUltimoLogin).HasColumnName("fecha_ultimo_login");

                entity.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");

                // ✅ Relación con Cliente (solo si la tabla existe)
                // Si la tabla cliente no existe, comenta esta línea
                entity.HasOne(u => u.Cliente)
                    .WithMany()
                    .HasForeignKey(u => u.IdCliente)
                    .HasConstraintName("fk_usuario_cliente_cliente")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // 🔧 CONFIGURACIÓN DE IDENTITY (Roles, Claims, etc.)
            // ============================================================

            // Roles
            modelBuilder.Entity<IdentityRole<int>>(entity =>
            {
                entity.ToTable("roles");
                entity.Property(r => r.Id).HasColumnName("id");
                entity.Property(r => r.Name).HasColumnName("name");
                entity.Property(r => r.NormalizedName).HasColumnName("normalized_name");
                entity.Property(r => r.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            });

            // Usuario Roles
            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.ToTable("usuario_roles");
                entity.Property(ur => ur.UserId).HasColumnName("user_id");
                entity.Property(ur => ur.RoleId).HasColumnName("role_id");
            });

            // Usuario Claims
            modelBuilder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("usuario_claims");
                entity.Property(uc => uc.Id).HasColumnName("id");
                entity.Property(uc => uc.UserId).HasColumnName("user_id");
                entity.Property(uc => uc.ClaimType).HasColumnName("claim_type");
                entity.Property(uc => uc.ClaimValue).HasColumnName("claim_value");
            });

            // Usuario Logins
            modelBuilder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("usuario_logins");
            });

            // Usuario Tokens
            modelBuilder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("usuario_tokens");
            });

            // Rol Claims
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.ToTable("rol_claims");
                entity.Property(rc => rc.Id).HasColumnName("id");
                entity.Property(rc => rc.RoleId).HasColumnName("role_id");
                entity.Property(rc => rc.ClaimType).HasColumnName("claim_type");   // 🔑 CLAVE
                entity.Property(rc => rc.ClaimValue).HasColumnName("claim_value"); // 🔑 CLAVE
            });

            // ============================================================
            // 🔧 CONFIGURACIONES DE TABLAS EXISTENTES
            // ============================================================

            // ✅ VentaTemporal
            modelBuilder.Entity<VentaTemporal>(entity =>
            {
                entity.ToTable("venta_temporal");
                entity.HasKey(e => e.IdVentaTemporal);
                entity.Property(e => e.IdVentaTemporal).HasColumnName("idventatemporal");
            });

            // ✅ Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("cliente");
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).HasColumnName("idcliente");
            });

            // ✅ Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("producto");
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.IdProducto).HasColumnName("idproducto");
            });

            // ✅ ProductoTienda
            modelBuilder.Entity<ProductoTienda>(entity =>
            {
                entity.ToTable("productotienda");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");

                entity.HasOne(pt => pt.Producto)
                    .WithMany()
                    .HasForeignKey(pt => pt.IdProducto)
                    .HasConstraintName("fk_productotienda_producto");
            });

            // ✅ Subcategoria
            modelBuilder.Entity<Subcategoria>(entity =>
            {
                entity.ToTable("subcategoria");
                entity.HasKey(e => e.IdSubcategoria);
                entity.Property(e => e.IdSubcategoria).HasColumnName("idsubcategoria");
            });

            // ✅ Categoriapropia
            modelBuilder.Entity<Categoriapropia>(entity =>
            {
                entity.ToTable("categoriapropia");
                entity.HasKey(e => e.IdCategoriapropia);
                entity.Property(e => e.IdCategoriapropia).HasColumnName("idcategoriapropia");
            });

            // ✅ Categoriapagina
            modelBuilder.Entity<Categoriapagina>(entity =>
            {
                entity.ToTable("categoriapagina");
                entity.HasKey(e => e.IdCategoriapagina);
                entity.Property(e => e.IdCategoriapagina).HasColumnName("idcategoriapagina");
            });

            // ✅ Subcategoriapagina
            modelBuilder.Entity<Subcategoriapagina>(entity =>
            {
                entity.ToTable("subcategoriapagina");
                entity.HasKey(e => e.IdSubcategoriapagina);
                entity.Property(e => e.IdSubcategoriapagina).HasColumnName("idsubcategoriapagina");
            });

            // ✅ Marca
            modelBuilder.Entity<Marca>(entity =>
            {
                entity.ToTable("tbmarcas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
            });

            // ✅ Medida
            modelBuilder.Entity<Medida>(entity =>
            {
                entity.ToTable("tbmedidas");
                entity.HasKey(e => e.IdMedidas);
                entity.Property(e => e.IdMedidas).HasColumnName("idmedidas");
            });

            // ✅ Kardex
            modelBuilder.Entity<Kardex>(entity =>
            {
                entity.ToTable("kardex");
                entity.HasKey(e => e.IdKardex);
                entity.Property(e => e.IdKardex).HasColumnName("idkardex");
            });

            // ✅ Tienda
            modelBuilder.Entity<Tienda>(entity =>
            {
                entity.ToTable("tblocal");
                entity.HasKey(e => e.IdLocal);
                entity.Property(e => e.IdLocal).HasColumnName("idlocal");
            });
        }
    }
}