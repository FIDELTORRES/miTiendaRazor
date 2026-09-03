using System.Threading.Tasks;

namespace miTienda.Services
{
    /// <summary>
    /// Interfaz para el servicio de envío de correos electrónicos
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico a un destinatario
        /// </summary>
        /// <param name="to">Correo electrónico del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="htmlMessage">Mensaje en formato HTML</param>
        /// <returns>Task</returns>
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}