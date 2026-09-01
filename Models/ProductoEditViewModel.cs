using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace miTienda.Models;

public class ProductoEditViewModel
{
    // ============================================================
    // 📌 DATOS DEL PRODUCTO
    // ============================================================
    public int IdProducto { get; set; }

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

    public string? Imagen { get; set; }  // Nombre de la imagen actual
    public IFormFile? ImagenFile { get; set; }  // Nueva imagen (opcional)

    // ============================================================
    // 📌 CLASIFICACIÓN OFICIAL (UNSPSC)
    // ============================================================
    [Required(ErrorMessage = "Seleccione una categoría oficial")]
    public int IdSubcategoria { get; set; }

    // ============================================================
    // 📌 STOCK Y PRECIOS
    // ============================================================
    public int? IdCategoria { get; set; }  // 🔧 IdCategoria (UNSPSC)
    public int? IdMedidas { get; set; }
    public decimal? StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public decimal? MargenPorcentaje { get; set; }

    // ============================================================
    // 📌 CONTROL
    // ============================================================
    public DateOnly? FechaIngreso { get; set; }
    public int? Estado { get; set; } = 1;

    // ============================================================
    // 📌 CLASIFICACIÓN TIENDA (WEB)
    // ============================================================
    public int? Categorias { get; set; }
    public int? CategoriaPaginaId { get; set; }
    public int? IdSubcategoriaPagina { get; set; }
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
    // 📌 DATOS DE PRODUCTO TIENDA (para mostrar)
    // ============================================================
    public List<ProductoTiendaEdit> ProductosTienda { get; set; } = new();
}

public class ProductoTiendaEdit
{
    public int Id { get; set; }
    public int IdTienda { get; set; }
    public string? NombreTienda { get; set; }
    public string? Codbarra { get; set; }
    public decimal? Stock { get; set; }
    public decimal? PrecioVenta { get; set; }
    public decimal? PrecioCompra { get; set; }
    public int? Estado { get; set; }
}