using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    [Table("venta_temporal")]
    public class VentaTemporal
    {
        [Key]
        [Column("idventatemporal")]
        public int IdVentaTemporal { get; set; }

        [Column("fecha")]
        public DateTime? Fecha { get; set; }

        [Column("tipocomprobante")]
        [MaxLength(2)]
        public string? TipoComprobante { get; set; }

        [Column("idtipofactura")]
        [MaxLength(4)]
        public string? IdTipoFactura { get; set; }

        [Column("idtipovalorventa")]
        [MaxLength(4)]
        public string? IdTipoValorVenta { get; set; }

        [Column("serie")]
        [MaxLength(2)]
        public string? Serie { get; set; }

        [Column("nrocomprobante")]
        public int? NroComprobante { get; set; }

        [Column("idtipoafectacion")]
        [MaxLength(4)]
        public string? IdTipoAfectacion { get; set; }

        [Column("idcliente")]
        [MaxLength(14)]
        public string? IdCliente { get; set; }

        [Column("estadocomprobante")]
        public int? EstadoComprobante { get; set; }

        [Column("igv")]
        public decimal? Igv { get; set; }

        [Column("subtotalventa")]
        public decimal? SubtotalVenta { get; set; }

        [Column("totalventa")]
        public decimal? TotalVenta { get; set; }

        [Column("idcodigodetalle")]
        [MaxLength(2)]
        public string? IdCodigoDetalle { get; set; }

        [Column("idmoneda")]
        public int? IdMoneda { get; set; }

        [Column("idempleado")]
        public int? IdEmpleado { get; set; }

        [Column("idisc")]
        [MaxLength(2)]
        public string? IdIsc { get; set; }

        [Column("totalgratuitas")]
        public decimal? TotalGratuitas { get; set; }

        [Column("totalexoneradas")]
        public decimal? TotalExoneradas { get; set; }

        [Column("totalinafecta")]
        public decimal? TotalInafecta { get; set; }

        [Column("hora")]
        public TimeOnly? Hora { get; set; }

        [Column("totalisc")]
        public decimal? TotalIsc { get; set; }

        [Column("usuarioingreso")]
        [MaxLength(20)]
        public string? UsuarioIngreso { get; set; }  // ← Usamos esto para SessionId

        [Column("usuariomodifico")]
        [MaxLength(20)]
        public string? UsuarioModifico { get; set; }

        [Column("fechaingreso")]
        public DateTime? FechaIngreso { get; set; }

        [Column("fechamodifica")]
        public DateTime? FechaModifica { get; set; }

        [Column("rucemisor")]
        [MaxLength(14)]
        public string? RucEmisor { get; set; }

        [Column("idcajero")]
        [MaxLength(60)]
        public string? IdCajero { get; set; }

        [Column("idlocal")]
        [MaxLength(6)]
        public string? IdLocal { get; set; }

        [Column("tipopago")]
        public int TipoPago { get; set; } = 2; // Contado por defecto

        [Column("tipoentrega")]
        public int? TipoEntrega { get; set; }

        [Column("indicaciones")]
        public string? Indicaciones { get; set; }

        [Column("fechaentrega")]
        public DateOnly? FechaEntrega { get; set; }

        [Column("horaentrega")]
        public TimeOnly? HoraEntrega { get; set; }

        [Column("direccionentrega")]
        [MaxLength(100)]
        public string? DireccionEntrega { get; set; }

        [Column("estadopedido")]
        public int? EstadoPedido { get; set; }

        [Column("idtransaccion")]
        [MaxLength(255)]
        public string? IdTransaccion { get; set; }

        [Column("nrocaja")]
        public decimal? NroCaja { get; set; }

        [Column("tipodoc")]
        public int? TipoDoc { get; set; }

        [Column("totalibcper")]
        public decimal? TotalIbcPer { get; set; }

        [Column("nrocuotas")]
        public int? NroCuotas { get; set; }

        [Column("idturno")]
        public int? IdTurno { get; set; }

        [Column("mediopago")]
        public int? MedioPago { get; set; }

        [Column("montocreditro")]
        public decimal? MontoCreditro { get; set; }

        [Column("nrooperacion")]
        [MaxLength(10)]
        public string? NroOperacion { get; set; }

        [Column("tipotarjeta")]
        public int? TipoTarjeta { get; set; }

        [Column("emisor")]
        [MaxLength(20)]
        public string? Emisor { get; set; }

        [Column("idusuariovendedor")]
        [MaxLength(10)]
        public string? IdUsuarioVendedor { get; set; }

        [Column("idvendedor")]
        [MaxLength(8)]
        public string? IdVendedor { get; set; }

        // Campos del producto (para el carrito)
        [Column("idproducto")]
        public int? IdProducto { get; set; }

        [Column("codbarra")]
        [MaxLength(50)]
        public string? Codbarra { get; set; }

        [Column("cantidad")]
        public decimal? Cantidad { get; set; }

        [Column("descripcion")]
        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Column("nombre")]
        [MaxLength(100)]
        public string? Nombre { get; set; }

        [Column("imagen")]
        [MaxLength(100)]
        public string? Imagen { get; set; }

        [Column("idcategoria")]
        public int? IdCategoria { get; set; }

        [Column("idtipotributo")]
        public int? IdTipoTributo { get; set; }

        [Column("impuesto")]
        public int? Impuesto { get; set; }

        [Column("idmedidas")]
        public int? IdMedidas { get; set; }

        [Column("preciocompra")]
        public decimal? PrecioCompra { get; set; }

        [Column("precioventa")]
        public decimal? PrecioVenta { get; set; }

        [Column("rutaimagen")]
        [MaxLength(255)]
        public string? RutaImagen { get; set; }

        [Column("corretemporal")]
        public decimal? CorreTemporal { get; set; }

        // 🔧 Propiedad auxiliar NO mapeada (solo para la aplicación)
        [NotMapped]
        public string? SessionId 
        { 
            get => UsuarioIngreso; 
            set => UsuarioIngreso = value; 
        }
    }
}