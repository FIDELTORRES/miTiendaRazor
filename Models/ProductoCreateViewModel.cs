using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace miTienda.Models;

public class ProductoCreateViewModel
{
    // ============================================================
    // 📌 DATOS DEL PRODUCTO
    // ============================================================
    [Required(ErrorMessage = "El código es obligatorio")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Codbarra { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    public string? Imagen { get; set; }
    public IFormFile? ImagenFile { get; set; }

    // ============================================================
    // 📌 CLASIFICACIÓN OFICIAL (UNSPSC)
    // ============================================================
    [Required(ErrorMessage = "Seleccione una categoría oficial")]
    public int IdSubcategoria { get; set; }

    // ============================================================
    // 📌 STOCK Y PRECIOS
    // ============================================================
    public int? IdMedidas { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal? MargenPorcentaje { get; set; }

    // ============================================================
    // 📌 PRECIOS DE VENTA Y COMPRA
    // ============================================================
    // public decimal? PrecioVenta { get; set; }
    // public decimal? PrecioCompra { get; set; }

    // ============================================================
    // 📌 CONTROL
    // ============================================================
    public DateOnly? FechaIngreso { get; set; }
    public int? Estado { get; set; } = 1;

    // ============================================================
    // 📌 CLASIFICACIÓN TIENDA (WEB)
    // ============================================================
    public int? Categorias { get; set; }  // Categoriapropia
    public int? CategoriaPaginaId { get; set; }  // Categoriapagina
    public int? IdSubcategoriaPagina { get; set; }  // 🔧 Subcategoriapagina
    public int? IdMarca { get; set; }

    // ============================================================
    // 📌 DATOS SUNAT
    // ============================================================
    [MaxLength(4)]
    public string? IdTipoAfectacion { get; set; } = "10";

    [MaxLength(4)]
    public string? IdTipoFactura { get; set; } = "0101";

    [MaxLength(4)]
    public string? IdCodigoDetalle { get; set; } = "01";

    [MaxLength(4)]
    public string? IdTipoTributo { get; set; } = "1000";

    [MaxLength(4)]
    public string? IdTipoValorVenta { get; set; } = "01";

    public decimal? Impuesto { get; set; } = 18.00m;

    // ============================================================
    // 📌 DATOS DE TIENDA
    // ============================================================
    public int IdTienda { get; set; } = 2;

    // 🔧 Este campo se usa en el formulario pero se ignora al guardar
    // El stock siempre será 0 en todas las tiendas según prompt
    public decimal? StockInicial { get; set; } = 0;
}