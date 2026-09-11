using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Services
{
    /// <summary>
    /// Servicio para enviar mensajes por WhatsApp usando Meta Cloud API
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly string _apiVersion;
        private readonly string _adminPhone;

        public WhatsAppService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();

            _accessToken = _configuration["WhatsApp:AccessToken"] ?? "";
            _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? "";
            _apiVersion = _configuration["WhatsApp:ApiVersion"] ?? "v20.0";
            _adminPhone = _configuration["WhatsApp:AdminPhone"] ?? "";
        }

        /// <summary>
        /// Envía un mensaje de texto simple
        /// </summary>
        public async Task<bool> EnviarMensajeTextoAsync(string telefono, string mensaje)
        {
            try
            {
                // Limpiar el número: solo dígitos (sin +, espacios, guiones)
                telefono = LimpiarTelefono(telefono);

                var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

                var body = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = telefono,
                    type = "text",
                    text = new { body = mensaje }
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error WhatsApp ({response.StatusCode}): {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error enviando WhatsApp: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía una imagen con caption
        /// </summary>
        public async Task<bool> EnviarImagenAsync(string telefono, string imagenUrl, string caption)
        {
            try
            {
                telefono = LimpiarTelefono(telefono);

                var url = $"https://graph.facebook.com/{_apiVersion}/{_phoneNumberId}/messages";

                var body = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = telefono,
                    type = "image",
                    image = new { link = imagenUrl, caption = caption }
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error enviando imagen: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía notificación al cliente
        /// </summary>
        public async Task<bool> EnviarPedidoAlClienteAsync(
            string telefono, int idPedido, string nombreCliente, decimal total,
            string direccion, string fechaEntrega, string horaEntrega)
        {
            var mensaje = $@"🛒 *¡Pedido Confirmado!*

Hola *{nombreCliente}*, tu pedido ha sido registrado exitosamente.

📦 *Pedido:* #{idPedido}
💰 *Total:* S/ {total:F2}
📍 *Dirección:* {direccion}
📅 *Entrega:* {fechaEntrega} - {horaEntrega}

¡Gracias por tu compra! 🎉
_Elfide.com_";

            return await EnviarMensajeTextoAsync(telefono, mensaje);
        }

        /// <summary>
        /// Envía notificación al administrador
        /// </summary>
        public async Task<bool> EnviarPedidoAlAdminAsync(
            int idPedido, string emailCliente, decimal total, string codigoTransaccion,
            string direccion, string fechaEntrega, string horaEntrega)
        {
            var mensaje = $@"🔔 *NUEVO PEDIDO WEB*

⚠️ Requiere atención del equipo de caja

📦 *Pedido:* #{idPedido}
👤 *Cliente:* {emailCliente}
💰 *Total:* S/ {total:F2}
💳 *Transacción:* {codigoTransaccion}
📍 *Dirección:* {direccion}
📅 *Entrega:* {fechaEntrega} - {horaEntrega}

🔗 Revisar en el sistema de caja.";

            return await EnviarMensajeTextoAsync(_adminPhone, mensaje);
        }

        /// <summary>
        /// Limpia el número de teléfono (solo dígitos)
        /// </summary>
        private string LimpiarTelefono(string telefono)
        {
            if (string.IsNullOrEmpty(telefono)) return telefono;

            // Si el número tiene 9 dígitos (Perú), agregar código de país 51
            var soloDigitos = new string(Array.FindAll(telefono.ToCharArray(), char.IsDigit));

            if (soloDigitos.Length == 9)
            {
                soloDigitos = "51" + soloDigitos;
            }

            return soloDigitos;
        }
    }
}