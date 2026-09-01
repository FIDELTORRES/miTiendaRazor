using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    /// <summary>
    /// Modelo para la tabla pagos_mercadopago
    /// </summary>
    [Table("pagos_mercadopago")]
    public class PagoMercadoPago
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("external_reference")]
        [MaxLength(40)]
        public string? ExternalReference { get; set; }

        [Column("collection_id")]
        [MaxLength(20)]
        public string? CollectionId { get; set; }

        [Column("collection_status")]
        [MaxLength(20)]
        public string? CollectionStatus { get; set; }

        [Column("payment_id")]
        [MaxLength(20)]
        public string? PaymentId { get; set; }

        [Column("estado")]
        [MaxLength(20)]
        public string? Estado { get; set; }

        [Column("payment_type")]
        [MaxLength(20)]
        public string? PaymentType { get; set; }

        [Column("merchant_order_id")]
        [MaxLength(20)]
        public string? MerchantOrderId { get; set; }

        [Column("preference_id")]
        [MaxLength(100)]
        public string? PreferenceId { get; set; }

        [Column("site_id")]
        [MaxLength(10)]
        public string? SiteId { get; set; }

        [Column("processing_mode")]
        [MaxLength(20)]
        public string? ProcessingMode { get; set; }

        [Column("merchant_account_id")]
        [MaxLength(20)]
        public string? MerchantAccountId { get; set; }

        [Column("metodopago")]
        public int? MetodoPago { get; set; }

        [Column("tipotarjeta")]
        public int? TipoTarjeta { get; set; }

        [Column("fecha_registro")]
        public DateTime? FechaRegistro { get; set; }

        [Column("idventa")]
        public int? IdVenta { get; set; }
    }
}
