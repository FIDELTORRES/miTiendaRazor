using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("tbmarcas")]
public class Marca
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("descripcion")]
    [MaxLength(60)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("estado")]
    public int? Estado { get; set; }

    [Column("flagtipo")]
    public int? FlagTipo { get; set; }
}