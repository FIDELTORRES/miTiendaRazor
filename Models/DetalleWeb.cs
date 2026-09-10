using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    /// <summary>
    /// Modelo para la tabla detalleweb (detalle de ventas)
    /// </summary>
    [Table("detalleweb")]
    public class DetalleWeb
    {
        [Key]
        [Column("iddetalleweb")]
        public int IdDetalleWeb { get; set; }

        [Column("idpedidoweb")]
        public int IdPedidoWeb { get; set; }

        [Column("seriecomprobante")]
        [MaxLength(4)]
        public string? SerieComprobante { get; set; }

        [Column("nrocomprobante")]
        public int? NroComprobante { get; set; }

        [Column("idproducto")]
        public int IdProducto { get; set; }

        [Column("codbarra")]
        [MaxLength(50)]
        public string? Codbarra { get; set; }

        [Column("idmedidas")]
        public int? IdMedidas { get; set; }

        [Column("cantidad")]
        public decimal? Cantidad { get; set; }

        [Column("precioventa")]
        public decimal? PrecioVenta { get; set; }

        [Column("descuento")]
        public decimal? Descuento { get; set; }

        [Column("importetotal")]
        public decimal? ImporteTotal { get; set; }

        [Column("descripcion")]
        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Column("fecharegistro")]
        public DateTime? FechaRegistro { get; set; }

        [Column("igv")]
        public decimal? Igv { get; set; }

        [Column("tipocodigoprecio")]
        [MaxLength(2)]
        public string? TipoCodigoPrecio { get; set; }

        [Column("isc")]
        public decimal? Isc { get; set; }

        [Column("idtipoafectacion")]
        [MaxLength(4)]
        public string? IdTipoAfectacion { get; set; }

        [Column("idtipovalorventa")]
        [MaxLength(4)]
        public string? IdTipoValorVenta { get; set; }

        [Column("idcodigodetalle")]
        [MaxLength(4)]
        public string? IdCodigoDetalle { get; set; }

        [Column("idisc")]
        public string? IdIsc { get; set; }

        [Column("mtotriotroitem")]
        public decimal? MtotrioTroItem { get; set; }

        [Column("codtriigv")]
        [MaxLength(4)]
        public string? CodTriIgv { get; set; }

        [Column("idtipooperacion")]
        [MaxLength(4)]
        public string? IdTipoOperacion { get; set; }

        [Column("nrooc")]
        public int? NroOc { get; set; }

        [Column("subtotal")]
        public decimal? Subtotal { get; set; }

        [Column("dirimagen")]
        [MaxLength(100)]
        public string? DirImagen { get; set; }

        [Column("indicaciones")]
        public string? Indicaciones { get; set; }

        [Column("fecha")]
        public DateOnly? Fecha { get; set; }

        [Column("idtienda")]
        public int IdTienda { get; set; }

        [Column("preciocompra")]
        public decimal? PrecioCompra { get; set; }

        [Column("precioreal")]
        public decimal? PrecioReal { get; set; }

        [Column("estadoproducto")]
        public int? EstadoProducto { get; set; }

        [Column("codigosunat")]
        [MaxLength(50)]
        public string? CodigoSunat { get; set; }

    }
}
