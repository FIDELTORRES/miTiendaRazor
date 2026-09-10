using Microsoft.AspNetCore.Identity;      // Identity para manejo de usuarios
using Microsoft.AspNetCore.Mvc;           // Para ActionResult
using Microsoft.AspNetCore.Mvc.RazorPages; // Para PageModel
using miTienda.Models;                    // Modelo UsuarioCliente
using System.ComponentModel.DataAnnotations; // Validaciones
using System.Threading.Tasks;

namespace miTienda.Pages.Auth
{
    /// <summary>
    /// Página para restablecer la contraseña (después de hacer clic en el enlace)
    /// </summary>
    public class ResetPasswordModel : PageModel
    {
        // Servicio para gestionar usuarios
        private readonly UserManager<UsuarioCliente> _userManager;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ResetPasswordModel(UserManager<UsuarioCliente> userManager)
        {
            _userManager = userManager;
        }

        // ============================================================
        // 📌 PROPIEDADES BIND (se enlazan con los datos del formulario)
        // ============================================================

        /// <summary>
        /// Correo electrónico del usuario (viene de la URL o formulario)
        /// </summary>
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Token de restablecimiento (viene de la URL)
        /// </summary>
        [BindProperty]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña ingresada por el usuario
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la nueva contraseña
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // ============================================================
        // 📌 PROPIEDADES PARA MENSAJES
        // ============================================================

        /// <summary>
        /// Mensaje de error que se muestra al usuario
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        // ============================================================
        // 📌 MÉTODO GET (cuando se carga la página desde el enlace)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta cuando el usuario hace clic en el enlace del correo
        /// </summary>
        /// <param name="email">Email del usuario (viene en la URL)</param>
        /// <param name="token">Token de restablecimiento (viene en la URL)</param>
        public IActionResult OnGet(string? email, string? token)
        {
            // 1. ✅ Verificar que el enlace tenga email y token
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ErrorMessage = "❌ Enlace inválido o expirado.";
                return Page();
            }

            // 2. ✅ Guardar los valores para usarlos en el formulario
            Email = email;
            Token = token;

            // 3. ✅ Mostrar el formulario para ingresar la nueva contraseña
            return Page();
        }

        // ============================================================
        // 📌 MÉTODO POST (cuando el usuario envía la nueva contraseña)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta cuando el usuario envía la nueva contraseña
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. ✅ Verificar que los datos del formulario sean válidos
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 2. 🔍 Buscar al usuario por su correo
            var user = await _userManager.FindByEmailAsync(Email);

            // 3. ✅ Verificar que el usuario exista
            if (user == null)
            {
                ErrorMessage = "❌ Usuario no encontrado.";
                return Page();
            }

            // 4. 🔑 Restablecer la contraseña usando el token
            var result = await _userManager.ResetPasswordAsync(user, Token, NewPassword);

            // 5. ✅ Si la operación fue exitosa
            if (result.Succeeded)
            {
                Success = true; // Mostrar mensaje de éxito
                return Page();
            }

            // 6. ❌ Si hubo errores, mostrarlos al usuario
            foreach (var error in result.Errors)
            {
                // Si el token es inválido o expiró
                if (error.Code == "InvalidToken")
                {
                    ErrorMessage = "❌ El enlace ha expirado o es inválido. Solicita uno nuevo.";
                    return Page();
                }
                // Otros errores (ej: contraseña muy débil)
                ErrorMessage = error.Description;
                return Page();
            }

            return Page();
        }
    }
}