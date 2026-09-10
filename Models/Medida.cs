using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("tbmedidas")]
public class Medida
{
    [Key]
    [Column("idmedidas")]
    public int IdMedidas { get; set; }

    [Column("descripcorta")]
    [MaxLength(6)]
    public string? DescripCorta { get; set; }

    [Column("descripcion")]
    [MaxLength(30)]
    public string? Descripcion { get; set; }

    [Column("estado")]
    public int? Estado { get; set; }
}