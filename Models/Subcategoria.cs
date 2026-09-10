using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("subcategoria")]
public class Subcategoria
{
    [Key]
    [Column("idsubcategoria")]
    public int IdSubcategoria { get; set; }

    [Column("idcategoria")]
    public int IdCategoria { get; set; }

    [Column("descripcion")]
    [MaxLength(150)]
    public string? Descripcion { get; set; }

    [Column("estado")]
    public int Estado { get; set; }

    [Column("uso")]
    public int? Uso { get; set; }

    [Column("imagen")]
    [MaxLength(100)]
    public string? Imagen { get; set; }
}