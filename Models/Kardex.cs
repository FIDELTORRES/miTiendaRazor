using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models;

[Table("kardex")]
public class Kardex
{
    [Key]
    [Column("idkardex")]
    public int IdKardex { get; set; }

    [Column("idproducto")]
    [MaxLength(50)]
    public string IdProducto { get; set; } = string.Empty;

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("cantidad")]
    public decimal Cantidad { get; set; }

    [Column("cantidadsalida")]
    public decimal? CantidadSalida { get; set; }

    [Column("fecha")]
    public DateOnly? Fecha { get; set; }

    [Column("hora")]
    public TimeOnly? Hora { get; set; }

    [Column("idalmacen")]
    public int? IdAlmacen { get; set; }

    [Column("idlocal")]
    public int? IdLocal { get; set; }

    [Column("estado")]
    public int Estado { get; set; }

    [Column("rucemisor")]
    [MaxLength(14)]
    public string RucEmisor { get; set; } = string.Empty;

    [Column("idusuario")]
    [MaxLength(20)]
    public string? IdUsuario { get; set; }

    [Column("observacion")]
    [MaxLength(100)]
    public string? Observacion { get; set; }

    [Column("preciounitario")]
    public decimal? PrecioUnitario { get; set; }
}