using Microsoft.EntityFrameworkCore;

namespace miTienda.Models;

public class MiTiendaContext : DbContext
{
    public MiTiendaContext(DbContextOptions<MiTiendaContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<ProductoTienda> ProductosTienda { get; set; } 

// Agregar estas propiedades al final de la clase MiTiendaContext
    public DbSet<Subcategoria> Subcategorias { get; set; }
    public DbSet<Categoriapropia> CategoriasPropias { get; set; }
    public DbSet<Categoriapagina> CategoriasPagina { get; set; }
    public DbSet<Subcategoriapagina> SubcategoriasPagina { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Medida> Medidas { get; set; }
    public DbSet<Kardex> Kardex { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuramos la tabla manualmente
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("producto");
            entity.HasKey(e => e.IdProducto);

            // Mapeo de columnas con sus tipos exactos
            entity.Property(e => e.IdProducto).HasColumnName("idproducto");
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(50);
            entity.Property(e => e.Codbarra).HasColumnName("codbarra").HasMaxLength(50);
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(255);
            entity.Property(e => e.Imagen).HasColumnName("imagen").HasMaxLength(100);
            entity.Property(e => e.IdCategoria).HasColumnName("idcategoria");
            entity.Property(e => e.IdPresentacion).HasColumnName("idpresentacion");
            entity.Property(e => e.IdSubcategoria).HasColumnName("idsubcategoria");
            entity.Property(e => e.CategoriaPaginaId).HasColumnName("categoriapagina_id");
            entity.Property(e => e.Categorias).HasColumnName("categorias");
            entity.Property(e => e.StockMinimo).HasColumnName("stockminimo").HasColumnType("decimal(12,2)");
            entity.Property(e => e.StockMaximo).HasColumnName("stockmaximo").HasColumnType("decimal(12,2)");
            entity.Property(e => e.MargenPorcentaje).HasColumnName("margenporcentaje").HasColumnType("decimal(6,2)");
            entity.Property(e => e.IdMedidas).HasColumnName("idmedidas");
            entity.Property(e => e.IdMarca).HasColumnName("idmarca");
            entity.Property(e => e.IdTipoAfectacion).HasColumnName("idtipoafectacion").HasMaxLength(4);
            entity.Property(e => e.IdTipoFactura).HasColumnName("idtipofactura").HasMaxLength(4);
            entity.Property(e => e.IdCodigoDetalle).HasColumnName("idcodigodetalle").HasMaxLength(4);
            entity.Property(e => e.IdTipoTributo).HasColumnName("idtipotributo").HasMaxLength(4);
            entity.Property(e => e.IdTipoValorVenta).HasColumnName("idtipovalorventa").HasMaxLength(4);
            entity.Property(e => e.Impuesto).HasColumnName("impuesto").HasColumnType("decimal(7,2)");
            entity.Property(e => e.RucEmisor).HasColumnName("rucemisor").HasMaxLength(14);
            entity.Property(e => e.FechaIngreso).HasColumnName("fechaingreso");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fechavencimiento");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FlagComprobante).HasColumnName("flagcomprobante");
            entity.Property(e => e.NombreArchi).HasColumnName("nombrearchi").HasMaxLength(150);
        });
        modelBuilder.Entity<ProductoTienda>(entity =>
        {
            entity.ToTable("productotienda");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RucEmisor).HasColumnName("rucemisor").HasMaxLength(14);
            entity.Property(e => e.IdTienda).HasColumnName("idtienda");
            entity.Property(e => e.Codbarra).HasColumnName("codbarra").HasMaxLength(50);
            entity.Property(e => e.Stock).HasColumnName("stock").HasColumnType("decimal(12,4)");
            entity.Property(e => e.PrecioVenta).HasColumnName("precioventa").HasColumnType("decimal(12,4)");
            entity.Property(e => e.PrecioCompra).HasColumnName("preciocompra").HasColumnType("decimal(12,4)");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.UsuarioRegistro).HasColumnName("usuarioregistro").HasMaxLength(50);
            entity.Property(e => e.FechaRegistro).HasColumnName("fecharegistro");
            entity.Property(e => e.IdProducto).HasColumnName("idproducto");
            entity.Property(e => e.FechaModifica).HasColumnName("fechamodifica");
            entity.Property(e => e.HoraModifica).HasColumnName("horamodifica");
            entity.Property(e => e.SubcategoriaId).HasColumnName("subcategoria_id");
            entity.Property(e => e.MarcaId).HasColumnName("marca_id");
            entity.Property(e => e.TallasId).HasColumnName("tallas_id");
            entity.Property(e => e.ModelosId).HasColumnName("modelos_id");
            entity.Property(e => e.ColoresId).HasColumnName("colores_id");

        });
        

        // Y en OnModelCreating agregar:
        modelBuilder.Entity<Subcategoria>(entity =>
        {
            entity.ToTable("subcategoria");
            entity.HasKey(e => e.IdSubcategoria);
            entity.Property(e => e.IdSubcategoria).HasColumnName("idsubcategoria");
            entity.Property(e => e.IdCategoria).HasColumnName("idcategoria");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.Uso).HasColumnName("uso");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
        });

        modelBuilder.Entity<Categoriapropia>(entity =>
        {
            entity.ToTable("categoriapropia");
            entity.HasKey(e => e.IdCategoriapropia);
            entity.Property(e => e.IdCategoriapropia).HasColumnName("idcategoriapropia");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
            entity.Property(e => e.Estado).HasColumnName("estado");
        });

        modelBuilder.Entity<Categoriapagina>(entity =>
        {
            entity.ToTable("categoriapagina");
            entity.HasKey(e => e.IdCategoriapagina);
            entity.Property(e => e.IdCategoriapagina).HasColumnName("idcategoriapagina");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.IdCategoriapropia).HasColumnName("idcategoriapropia");
            entity.Property(e => e.IdSubcategoria).HasColumnName("idsubcategoria");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
            entity.Property(e => e.Estado).HasColumnName("estado");
        });

        modelBuilder.Entity<Subcategoriapagina>(entity =>
        {
            entity.ToTable("subcategoriapagina");
            entity.HasKey(e => e.IdSubcategoriapagina);
            entity.Property(e => e.IdSubcategoriapagina).HasColumnName("idsubcategoriapagina");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdCategoriapagina).HasColumnName("idcategoriapagina");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.ToTable("tbmarcas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FlagTipo).HasColumnName("flagtipo");
        });

        modelBuilder.Entity<Medida>(entity =>
        {
            entity.ToTable("tbmedidas");
            entity.HasKey(e => e.IdMedidas);
            entity.Property(e => e.IdMedidas).HasColumnName("idmedidas");
            entity.Property(e => e.DescripCorta).HasColumnName("descripcorta");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Estado).HasColumnName("estado");
        });

        modelBuilder.Entity<Kardex>(entity =>
        {
            entity.ToTable("kardex");
            entity.HasKey(e => e.IdKardex);
            entity.Property(e => e.IdKardex).HasColumnName("idkardex");
            entity.Property(e => e.IdProducto).HasColumnName("idproducto");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CantidadSalida).HasColumnName("cantidadsalida");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Hora).HasColumnName("hora");
            entity.Property(e => e.IdAlmacen).HasColumnName("idalmacen");
            entity.Property(e => e.IdLocal).HasColumnName("idlocal");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.RucEmisor).HasColumnName("rucemisor");
            entity.Property(e => e.IdUsuario).HasColumnName("idusuario");
            entity.Property(e => e.Observacion).HasColumnName("observacion");
            entity.Property(e => e.PrecioUnitario).HasColumnName("preciounitario");
        });
    }
}