using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("categoriapropia")]
public class Categoriapropia
{
    [Key]
    [Column("idcategoriapropia")]
    public int IdCategoriapropia { get; set; }

    [Column("descripcion")]
    [MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("imagen")]
    [MaxLength(100)]
    public string? Imagen { get; set; }

    [Column("estado")]
    public int Estado { get; set; } = 1;
}