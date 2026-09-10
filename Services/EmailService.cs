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
            var sender = _configuration["Email:Sender"];
            var host = _configuration["Email:Host"];
            var portText = _configuration["Email:Port"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            if (string.IsNullOrWhiteSpace(sender) ||
                string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portText) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Faltan configuraciones de Email en appsettings.json.");
            }

            if (!int.TryParse(portText, out var port))
            {
                throw new InvalidOperationException("El puerto SMTP configurado no es válido.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Elfide.com", sender));
            email.To.Add(new MailboxAddress(string.Empty, to));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}