using MailKit.Net.Smtp;           // Cliente SMTP para enviar correos
using MailKit.Security;           // Opciones de seguridad SSL/TLS
using Microsoft.Extensions.Configuration; // Leer configuración de appsettings.json
using MimeKit;                    // Construir mensajes de correo
using System.Threading.Tasks;

namespace miTienda.Services
{
    /// <summary>
    /// Servicio para enviar correos electrónicos usando MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        // Configuración de la aplicación (lee appsettings.json)
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor que recibe la configuración por inyección de dependencias
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Envía un correo electrónico usando SMTP
        /// </summary>
        /// <param name="to">Correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="htmlMessage">Contenido HTML del correo</param>
        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            // 1. Crear un nuevo mensaje de correo
            var email = new MimeMessage();

            // 2. Configurar el remitente (desde appsettings.json)
            email.From.Add(new MailboxAddress("Elfide.com", _configuration["Email:Sender"]));

            // 3. Configurar el destinatario
            email.To.Add(new MailboxAddress("", to));

            // 4. Configurar el asunto del correo
            email.Subject = subject;

            // 5. Crear el cuerpo del mensaje en formato HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = bodyBuilder.ToMessageBody();

            // 6. Crear el cliente SMTP para enviar el correo
            using var smtp = new SmtpClient();

            // 7. Conectar al servidor SMTP (Gmail, Outlook, etc.)
            await smtp.ConnectAsync(
                _configuration["Email:Host"],                              // Servidor SMTP (ej: smtp.gmail.com)
                int.Parse(_configuration["Email:Port"]),                 // Puerto (587 para TLS)
                SecureSocketOptions.StartTls                            // Usar TLS para seguridad
            );

            // 8. Autenticarse con el servidor SMTP
            await smtp.AuthenticateAsync(
                _configuration["Email:Username"],                       // Tu correo
                _configuration["Email:Password"]                        // Tu contraseña o contraseña de aplicación
            );

            // 9. Enviar el correo
            await smtp.SendAsync(email);

            // 10. Desconectar del servidor SMTP
            await smtp.DisconnectAsync(true);
        }
    }
}