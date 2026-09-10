using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; 
using miTienda.Models;
using miTienda.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace miTienda.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<UsuarioCliente> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(UserManager<UsuarioCliente> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // ✅ 1. Validar el modelo
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // ✅ 2. Verificar que el email no esté vacío
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "❌ Por favor, ingresa tu correo electrónico.";
                    return Page();
                }

                // ✅ 3. Buscar usuario por email (USANDO LINQ - más seguro)
                UsuarioCliente? user = null;
                try
                {
                    // ⭐ USAR LINQ en lugar de FindByEmailAsync
                    user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == Email.Trim());
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al buscar el usuario: {ex.Message}";
                    return Page();
                }

                // ✅ 4. Si el usuario no existe, mensaje genérico (seguridad)
                if (user == null)
                {
                    SuccessMessage = "✅ Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.";
                    return Page();
                }

                // ✅ 5. Verificar que el usuario esté ACTIVO
                if (user.Estado != 1)
                {
                    ErrorMessage = "❌ Cuenta desactivada. Contacta al administrador.";
                    return Page();
                }

                // ✅ 6. Generar token de restablecimiento
                string token;
                try
                {
                    token = await _userManager.GeneratePasswordResetTokenAsync(user);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al generar el token: {ex.Message}";
                    return Page();
                }

                // ✅ 7. Construir enlace de restablecimiento
                var resetLink = Url.Page(
                    "/Auth/ResetPassword",
                    pageHandler: null,
                    values: new { email = Email.Trim(), token = token },
                    protocol: Request.Scheme
                );

                if (string.IsNullOrEmpty(resetLink))
                {
                    ErrorMessage = "❌ Error al generar el enlace de restablecimiento.";
                    return Page();
                }

                // ✅ 8. Construir el mensaje del correo
                var subject = "🔑 Restablece tu contraseña - Elfide.com";
                var htmlMessage = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #0B6299; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; border-radius: 0 0 8px 8px; }}
                            .btn {{ display: inline-block; background-color: #FFA600; color: #070B12; padding: 12px 24px; text-decoration: none; font-weight: bold; border-radius: 8px; }}
                            .footer {{ margin-top: 20px; text-align: center; font-size: 12px; color: #888; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>🔑 Recupera tu contraseña</h2>
                            </div>
                            <div class='content'>
                                <p>Hola <strong>{user.Nombres ?? "Usuario"}</strong>,</p>
                                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>Elfide.com</strong>.</p>
                                <p>Haz clic en el botón para crear una nueva contraseña:</p>
                                <p style='text-align: center;'>
                                    <a href='{resetLink}' class='btn'>Restablecer Contraseña</a>
                                </p>
                                <p>⚠️ Este enlace expirará en <strong>1 hora</strong>.</p>
                                <p>🔒 Si no solicitaste este cambio, ignora este mensaje.</p>
                                <hr />
                                <p style='font-size: 12px; color: #888;'>Elfide.com - Tienda Online</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                // ✅ 9. Enviar el correo
                try
                {
                    await _emailService.SendEmailAsync(Email.Trim(), subject, htmlMessage);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al enviar el correo: {ex.Message}";
                    return Page();
                }

                // ✅ 10. Mensaje de éxito
                SuccessMessage = "✅ Hemos enviado un enlace a tu correo. Revisa tu bandeja de entrada o spam.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ Error inesperado: {ex.Message}";
                return Page();
            }
        }
    }
}