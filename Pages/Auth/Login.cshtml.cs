using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace miTienda.Pages.Auth
{
    /// <summary>
    /// Página de inicio de sesión de clientes
    /// </summary>
    public class LoginModel : PageModel
    {
        // ============================================================
        // 📌 SERVICIOS
        // ============================================================

        private readonly SignInManager<UsuarioCliente> _signInManager;
        private readonly UserManager<UsuarioCliente> _userManager;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public LoginModel(SignInManager<UsuarioCliente> signInManager, UserManager<UsuarioCliente> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ============================================================
        // 📌 PROPIEDADES BIND (se enlazan con el formulario)
        // ============================================================

        [BindProperty]
        public LoginViewModel Login { get; set; } = new();

        /// <summary>
        /// URL de retorno después del login
        /// </summary>
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Mensaje de error que se muestra al usuario
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Mensaje de éxito que se muestra al usuario
        /// </summary>
        public string? SuccessMessage { get; set; }

        // ============================================================
        // 📌 MÉTODO GET (cuando se carga la página)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta cuando se carga la página de login
        /// </summary>
        public void OnGet(string? returnUrl = null, string? error = null, string? success = null)
        {
            ReturnUrl = returnUrl ?? "/";
            ErrorMessage = error;
            SuccessMessage = success;
        }

        // ============================================================
        // 📌 MÉTODO POST (cuando se envía el formulario)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta cuando el usuario envía el formulario de login
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // ✅ 1. Validar el modelo
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // ✅ 2. Validar que el email no esté vacío
                if (string.IsNullOrWhiteSpace(Login.Email))
                {
                    ErrorMessage = "❌ El correo electrónico es obligatorio.";
                    return Page();
                }

                // ✅ 3. Buscar usuario por email (LINQ directo, sin NormalizedEmail)
                UsuarioCliente? user = null;
                try
                {
                    user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == Login.Email.Trim());
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al buscar el usuario: {ex.Message}";
                    return Page();
                }

                // ✅ 4. Verificar si el usuario existe
                if (user == null)
                {
                    ErrorMessage = "❌ El correo electrónico no está registrado.";
                    return Page();
                }

                // ✅ 5. Verificar si el usuario está activo
                if (user.Estado != 1)
                {
                    ErrorMessage = "❌ Cuenta desactivada. Contacte al administrador.";
                    return Page();
                }

                // ✅ 6. Verificar que el usuario tenga UserName
                if (string.IsNullOrEmpty(user.UserName))
                {
                    ErrorMessage = "❌ Error en la configuración del usuario.";
                    return Page();
                }

                // ✅ 7. Intentar iniciar sesión
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,           // Username (email)
                    Login.Password,          // Contraseña
                    Login.RememberMe,        // Recordarme
                    lockoutOnFailure: true   // Bloquear después de intentos fallidos
                );

                // ✅ 8. Verificar resultado del login
                if (result.Succeeded)
                {
                    // ✅ 8.1 Actualizar fecha de último login
                    user.FechaUltimoLogin = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // ✅ 8.2 Redirigir a la página de retorno
                    return RedirectToLocal(ReturnUrl);
                }

                // ✅ 9. Manejar casos específicos
                if (result.IsLockedOut)
                {
                    ErrorMessage = "🔒 Cuenta bloqueada temporalmente. Intente más tarde.";
                    return Page();
                }

                if (result.IsNotAllowed)
                {
                    ErrorMessage = "⛔ No tienes permiso para iniciar sesión.";
                    return Page();
                }

                // ✅ 10. Si llegamos aquí, la contraseña es incorrecta
                ErrorMessage = "❌ Contraseña incorrecta.";
                return Page();
            }
            catch (Exception ex)
            {
                // ✅ 11. Capturar cualquier error inesperado
                ErrorMessage = $"❌ Error inesperado: {ex.Message}";
                return Page();
            }
        }

        // ============================================================
        // 📌 MÉTODOS AUXILIARES
        // ============================================================

        /// <summary>
        /// Redirige a la URL de retorno o a la página de inicio
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToPage("/Web/Index");
        }

        // ============================================================
        // 📌 CLASE VIEWMODEL
        // ============================================================

        /// <summary>
        /// ViewModel para el formulario de login
        /// </summary>
        public class LoginViewModel
        {
            [Required(ErrorMessage = "El correo es obligatorio")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }
    }
}