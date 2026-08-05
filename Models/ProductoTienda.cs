using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

/// <summary>
/// 📌 PRODUCTO POR TIENDA
/// Tabla: productotienda
/// 
/// RELACIONES:
/// - idproducto → Apunta a Producto.idproducto
/// - idtienda → Apunta a Tienda.idtienda (2 = Tienda Principal, 3 = Almacén)
/// </summary>
[Table("productotienda")]
public class ProductoTienda
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("rucemisor")]
    [MaxLength(14)]
    public string RucEmisor { get; set; } = string.Empty;

    [Column("idtienda")]
    public int IdTienda { get; set; }

    [Column("codbarra")]
    [MaxLength(50)]
    public string? Codbarra { get; set; }

    [Column("stock", TypeName = "decimal(12,4)")]
    public decimal? Stock { get; set; }

      [Column("precioventa", TypeName="decimal(12,4)")]

    public decimal? PrecioVenta { get; set; }

    [Column("preciocompra", TypeName = "decimal(12,4)")] 
    public decimal? PrecioCompra { get; set; }  

    [Column("estado")]
    public int Estado { get; set; }

    [Column("usuarioregistro")]
    [MaxLength(50)]
    public string? UsuarioRegistro { get; set; }

    [Column("fecharegistro")]
    public DateTime? FechaRegistro { get; set; }

    [Column("idproducto")]
    public int IdProducto { get; set; }

    [Column("fechamodifica")]
    public DateOnly? FechaModifica { get; set; }

    [Column("horamodifica")]
    public TimeOnly? HoraModifica { get; set; }

    [Column("subcategoria_id")]
    public int? SubcategoriaId { get; set; }

    [Column("marca_id")]
    public int? MarcaId { get; set; }

    [Column("tallas_id")]
    public int? TallasId { get; set; }

    [Column("modelos_id")]
    public int? ModelosId { get; set; }

    [Column("colores_id")]
    public int? ColoresId { get; set; }

    // ============================================================
    // NAVEGACIÓN (Relación con Producto)
    // ============================================================
    [ForeignKey("IdProducto")]
    public virtual Producto? Producto { get; set; }
}