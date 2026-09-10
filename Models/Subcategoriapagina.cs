using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("subcategoriapagina")]
public class Subcategoriapagina
{
    [Key]
    [Column("idsubcategoriapagina")]
    public int IdSubcategoriapagina { get; set; }

    [Column("descripcion")]
    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [Column("estado")]
    public int? Estado { get; set; } = 1;

    [Column("idcategoriapagina")]
    public int? IdCategoriapagina { get; set; }

    [Column("imagen")]
    [MaxLength(100)]
    public string? Imagen { get; set; }

    // 🔥 AGREGAR ESTA PROPIEDAD DE NAVEGACIÓN
    [ForeignKey("IdCategoriapagina")]
    public virtual Categoriapagina? Categoriapagina { get; set; }
}