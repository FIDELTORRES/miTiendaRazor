using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

/// <summary>
/// 📌 PRODUCTO PRINCIPAL
/// Tabla: producto
/// 
/// RELACIONES CON CATEGORÍAS:
/// - idcategoria → Apunta a Subcategoria.idsubcategoria (desde app categorias)
/// - idsubcategoria → Apunta a Subcategoria.idsubcategoria
/// - categoriapagina_id → Apunta a Categoriapagina.idcategoriapagina (desde app categorias)
/// - categorias → Apunta a Categoriapropia.idcategoriapropia (desde app categorias)
/// </summary>
[Table("producto")]
public class Producto
{
    [Key]
    [Column("idproducto")]
    public int IdProducto { get; set; }

    [Column("codigo")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Column("codbarra")]
    [MaxLength(50)]
    public string? Codbarra { get; set; }

    [Column("nombre")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [Column("imagen")]
    [MaxLength(100)]
    public string? Imagen { get; set; }

    // ============================================================
    // CLASIFICACIÓN (Relacionado con categorías)
    // ============================================================
    [Column("idcategoria")]
    public int IdCategoria { get; set; }

    [Column("idpresentacion")]
    public int? IdPresentacion { get; set; }

    [Column("idsubcategoria")]
    public int? IdSubcategoria { get; set; }

    [Column("categoriapagina_id")]
    public int? CategoriaPaginaId { get; set; }

    [Column("categorias")]
    public int? Categorias { get; set; }

    // ============================================================
    // STOCK
    // ============================================================
    [Column("stockminimo", TypeName = "decimal(12,2)")]
    public decimal? StockMinimo { get; set; }

    [Column("stockmaximo", TypeName = "decimal(12,2)")]
    public decimal? StockMaximo { get; set; }

    [Column("margenporcentaje", TypeName = "decimal(6,2)")]
    public decimal? MargenPorcentaje { get; set; }

    // ============================================================
    // RELACIONES CATÁLOGO
    // ============================================================
    [Column("idmedidas")]
    public int? IdMedidas { get; set; }

    [Column("idmarca")]
    public int? IdMarca { get; set; }

    // ============================================================
    // SUNAT (Facturación)
    // ============================================================
    [Column("idtipoafectacion")]
    [MaxLength(4)]
    public string? IdTipoAfectacion { get; set; }

    [Column("idtipofactura")]
    [MaxLength(4)]
    public string? IdTipoFactura { get; set; }

    [Column("idcodigodetalle")]
    [MaxLength(4)]
    public string? IdCodigoDetalle { get; set; }

    [Column("idtipotributo")]
    [MaxLength(4)]
    public string? IdTipoTributo { get; set; }

    [Column("idtipovalorventa")]
    [MaxLength(4)]
    public string? IdTipoValorVenta { get; set; }

    [Column("impuesto", TypeName = "decimal(7,2)")]
    public decimal? Impuesto { get; set; }

    // ============================================================
    // CONTROL
    // ============================================================
    [Column("rucemisor")]
    [MaxLength(14)]
    public string RucEmisor { get; set; } = string.Empty;

    [Column("fechaingreso")]
    public DateOnly? FechaIngreso { get; set; }

    [Column("fechavencimiento")]
    public DateOnly? FechaVencimiento { get; set; }

    [Column("estado")]
    public int? Estado { get; set; }

    [Column("flagcomprobante")]
    public int? FlagComprobante { get; set; }

    [Column("nombrearchi")]
    [MaxLength(150)]
    public string? NombreArchi { get; set; }
}