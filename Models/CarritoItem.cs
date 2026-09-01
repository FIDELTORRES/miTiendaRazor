using System.Text.Json.Serialization;

namespace miTienda.Models
{
    /// <summary>
    /// Representa un item en el carrito de compras
    /// </summary>
    public class CarritoItem
    {
        public int ProductoId { get; set; }
        public string? Nombre { get; set; }
        public string? Imagen { get; set; }
        public decimal? PrecioVenta { get; set; }
        public int Cantidad { get; set; }

        [JsonIgnore]
        public decimal? Subtotal => (PrecioVenta ?? 0) * Cantidad;
    }
}