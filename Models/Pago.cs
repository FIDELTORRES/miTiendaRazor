using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace miTienda.Models
{
    /// <summary>
    /// Modelo para la tabla pagos (PayPal, etc.)
    /// </summary>
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pedido_id")]
        [MaxLength(40)]
        public string? PedidoId { get; set; }

        [Column("pasarela")]
        [MaxLength(20)]
        public string? Pasarela { get; set; }  // 'MERCADOPAGO', 'PAYPAL', 'CULQI'

        [Column("transaccion_id")]
        [MaxLength(50)]
        public string? TransaccionId { get; set; }

        [Column("estado")]
        [MaxLength(20)]
        public string? Estado { get; set; }  // 'approved', 'COMPLETED', etc.

        [Column("monto_monto")]
        public decimal? Monto { get; set; }

        [Column("moneda")]
        [MaxLength(5)]
        public string? Moneda { get; set; }

        [Column("comision")]
        public decimal? Comision { get; set; }

        [Column("monto_neto")]
        public decimal? MontoNeto { get; set; }

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("idventa")]
        public int? IdVenta { get; set; }
    }
}
