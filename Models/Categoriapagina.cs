using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("categoriapagina")]
public class Categoriapagina
{
    [Key]
    [Column("idcategoriapagina")]
    public int IdCategoriapagina { get; set; }

    [Column("descripcion")]
    [MaxLength(60)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("idcategoriapropia")]
    public int IdCategoriapropia { get; set; }

    [Column("idsubcategoria")]
    [MaxLength(20)]
    public string IdSubcategoria { get; set; } = string.Empty;  // ← Volver a string

    [Column("imagen")]
    [MaxLength(100)]
    public string? Imagen { get; set; }

    [Column("estado")]
    public int Estado { get; set; } = 1;


    // ============================================================
    // 🔽 PROPIEDADES DE NAVEGACIÓN (RELACIONES)
    // ============================================================

    /// <summary>
    /// Relación con Categoriapropia (Nivel 1)
    /// </summary>
    /// [ForeignKey(nameof(IdCategoriapropia))]
    [ForeignKey("IdCategoriapropia")]
    public virtual Categoriapropia? Categoriapropia { get; set; }
}