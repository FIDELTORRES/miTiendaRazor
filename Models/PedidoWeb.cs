using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    /// <summary>
    /// Modelo para la tabla pedidoweb (cabecera de ventas)
    /// </summary>
    [Table("pedidoweb")]
    public class PedidoWeb
    {
        [Key]
        [Column("idpedidoweb")]
        public int IdPedidoWeb { get; set; }

        [Column("fecha")]
        public DateOnly? Fecha { get; set; }

        [Column("serie")]
        [MaxLength(4)]
        public string? Serie { get; set; }

        [Column("nrocomprobante")]
        public int? NroComprobante { get; set; }

        [Column("idcliente")]
        [MaxLength(14)]
        public string? IdCliente { get; set; }

        [Column("estadocomprobante")]
        public int? EstadoComprobante { get; set; }

        [Column("trimestre")]
        [MaxLength(20)]
        public string? Trimestre { get; set; }

        [Column("valorneto")]
        public decimal? ValorNeto { get; set; }

        [Column("descuento")]
        public decimal? Descuento { get; set; }

        [Column("totaldescuento")]
        public decimal? TotalDescuento { get; set; }

        [Column("tasaigv")]
        public decimal? TasaIgv { get; set; }

        [Column("igv")]
        public decimal? Igv { get; set; }

        [Column("subtotalventa")]
        public decimal? SubtotalVenta { get; set; }

        [Column("totalventa")]
        public decimal? TotalVenta { get; set; }

        [Column("idcodigodetalle")]
        [MaxLength(2)]
        public string? IdCodigoDetalle { get; set; }

        [Column("nrooc")]
        public int? NroOc { get; set; }

        [Column("idmoneda")]
        public int? IdMoneda { get; set; }

        [Column("idempleado")]
        public int? IdEmpleado { get; set; }

        [Column("ctacte")]
        [MaxLength(30)]
        public string? Ctacte { get; set; }

        [Column("hashsunat")]
        [MaxLength(100)]
        public string? HashSunat { get; set; }

        [Column("montoletras")]
        [MaxLength(200)]
        public string? MontoLetras { get; set; }

        [Column("fechavto")]
        public DateOnly? FechaVto { get; set; }

        [Column("nroguiaremision")]
        [MaxLength(6)]
        public string? NroGuiaRemision { get; set; }

        [Column("codguiaremision")]
        public int? CodGuiaRemision { get; set; }

        [Column("otronrocomprobante")]
        [MaxLength(6)]
        public string? OtroNroComprobante { get; set; }

        [Column("otrototalimpuesto")]
        public decimal? OtroTotalImpuesto { get; set; }

        [Column("idisc")]
        [MaxLength(2)]
        public string? IdIsc { get; set; }

        [Column("totalexportacion")]
        public decimal? TotalExportacion { get; set; }

        [Column("totalgratuitas")]
        public decimal? TotalGratuitas { get; set; }

        [Column("totalexoneradas")]
        public decimal? TotalExoneradas { get; set; }

        [Column("totalinafecta")]
        public decimal? TotalInafecta { get; set; }

        [Column("hora")]
        public TimeOnly? Hora { get; set; }

        [Column("totalanticipos")]
        public decimal? TotalAnticipos { get; set; }

        [Column("rptasunat")]
        public int? Rptasunat { get; set; }

        [Column("obsersunat")]
        public string? Obsersunat { get; set; }

        [Column("observacion")]
        public string? Observacion { get; set; }

        [Column("idnotacredito")]
        public int? IdNotaCredito { get; set; }

        [Column("idnotadebito")]
        public int? IdNotaDebito { get; set; }

        [Column("resumensunat")]
        public int? ResumenSunat { get; set; }

        [Column("totalisc")]
        public decimal? TotalIsc { get; set; }

        [Column("nompdf")]
        [MaxLength(100)]
        public string? NomPdf { get; set; }

        [Column("nomxml")]
        [MaxLength(100)]
        public string? NomXml { get; set; }

        [Column("imagenqr")]
        [MaxLength(100)]
        public string? ImagenQr { get; set; }

        [Column("ticketresumen")]
        [MaxLength(25)]
        public string? TicketResumen { get; set; }

        [Column("estadoresumen")]
        public int? EstadoResumen { get; set; }

        [Column("xmlsunat")]
        [MaxLength(100)]
        public string? XmlSunat { get; set; }

        [Column("usuarioingreso")]
        [MaxLength(20)]
        public string? UsuarioIngreso { get; set; }

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
        public int? TipoPago { get; set; }

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
    }
}
