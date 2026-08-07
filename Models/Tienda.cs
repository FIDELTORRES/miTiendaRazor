using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("tblocal")]
public class Tienda
{
    [Key]
    [Column("idlocal")]
    public int IdLocal { get; set; }

    [Column("nombre")]
    [MaxLength(20)]
    public string Nombre { get; set; } = string.Empty;

    [Column("direccion")]
    [MaxLength(100)]
    public string Direccion { get; set; } = string.Empty;

    [Column("estado")]
    public int? Estado { get; set; }

    [Column("rucemisor")]
    [MaxLength(14)]
    public string? RucEmisor { get; set; }

    [Column("flag")]
    public int? Flag { get; set; }
}