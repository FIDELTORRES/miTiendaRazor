using System.Threading.Tasks;

namespace miTienda.Services
{
    /// <summary>
    /// Interfaz para el servicio de envío de mensajes por WhatsApp
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// Envía un mensaje de texto simple
        /// </summary>
        Task<bool> EnviarMensajeTextoAsync(string telefono, string mensaje);

        /// <summary>
        /// Envía una imagen con un texto (caption)
        /// </summary>
        Task<bool> EnviarImagenAsync(string telefono, string imagenUrl, string caption);

        /// <summary>
        /// Envía la notificación de pedido al cliente
        /// </summary>
        Task<bool> EnviarPedidoAlClienteAsync(string telefono, int idPedido, string nombreCliente, decimal total, string direccion, string fechaEntrega, string horaEntrega);

        /// <summary>
        /// Envía la notificación de nuevo pedido al administrador
        /// </summary>
        Task<bool> EnviarPedidoAlAdminAsync(int idPedido, string emailCliente, decimal total, string codigoTransaccion, string direccion, string fechaEntrega, string horaEntrega);
    }
}