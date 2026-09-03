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
    /// Página de registro de nuevos clientes
    /// </summary>
    public class RegistroModel : PageModel
    {
        // ============================================================
        // 📌 SERVICIOS
        // ============================================================

        private readonly UserManager<UsuarioCliente> _userManager;
        private readonly SignInManager<UsuarioCliente> _signInManager;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public RegistroModel(UserManager<UsuarioCliente> userManager, SignInManager<UsuarioCliente> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ============================================================
        // 📌 PROPIEDADES BIND (se enlazan con el formulario)
        // ============================================================

        [BindProperty]
        public RegistroViewModel Registro { get; set; } = new();

        /// <summary>
        /// URL de retorno después del registro
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
        /// Método que se ejecuta cuando se carga la página de registro
        /// </summary>
        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        // ============================================================
        // 📌 MÉTODO POST (cuando se envía el formulario)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta cuando el usuario envía el formulario de registro
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
                if (string.IsNullOrWhiteSpace(Registro.Email))
                {
                    ErrorMessage = "❌ El correo electrónico es obligatorio.";
                    return Page();
                }

                // ✅ 3. Verificar si el email ya está registrado
                UsuarioCliente? existingUser = null;
                try
                {
                    existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == Registro.Email.Trim());
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al verificar el usuario: {ex.Message}";
                    return Page();
                }

                if (existingUser != null)
                {
                    ErrorMessage = "❌ Este correo electrónico ya está registrado.";
                    return Page();
                }

                // ✅ 4. Crear el usuario
                var usuario = new UsuarioCliente
                {
                    UserName = Registro.Email.Trim(),
                    Email = Registro.Email.Trim(),
                    Nombres = Registro.Nombres?.Trim(),
                    Apellidos = Registro.Apellidos?.Trim(),
                    Telefono = Registro.Telefono?.Trim(),
                    FechaRegistro = DateTime.Now,
                    Estado = 1, // Activo
                    EmailConfirmed = true, // Confirmado automáticamente
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                // ✅ 5. Intentar crear el usuario
                IdentityResult result;
                try
                {
                    result = await _userManager.CreateAsync(usuario, Registro.Password);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"❌ Error al crear el usuario: {ex.Message}";
                    return Page();
                }

                // ✅ 6. Verificar resultado
                if (result.Succeeded)
                {
                    // ✅ 6.1 Asignar rol "Cliente" por defecto (si existe)
                    try
                    {
                        // Verificar si el rol "Cliente" existe
                        var roleExists = await _userManager.GetUsersInRoleAsync("Cliente");
                        if (roleExists != null)
                        {
                            await _userManager.AddToRoleAsync(usuario, "Cliente");
                        }
                    }
                    catch
                    {
                        // Si falla la asignación de rol, no es crítico, el usuario ya está creado
                    }

                    // ✅ 6.2 Iniciar sesión automáticamente
                    await _signInManager.SignInAsync(usuario, isPersistent: false);

                    // ✅ 6.3 Redirigir a la página de retorno o al inicio
                    SuccessMessage = "✅ ¡Cuenta creada exitosamente!";
                    return RedirectToLocal(ReturnUrl);
                }

                // ✅ 7. Mostrar errores de Identity
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateEmail" || error.Code == "DuplicateUserName")
                    {
                        ErrorMessage = "❌ Este correo electrónico ya está registrado.";
                        return Page();
                    }
                    if (error.Code == "PasswordTooShort")
                    {
                        ErrorMessage = "❌ La contraseña debe tener al menos 6 caracteres.";
                        return Page();
                    }
                    if (error.Code == "PasswordRequiresNonAlphanumeric")
                    {
                        ErrorMessage = "❌ La contraseña debe tener al menos un carácter especial.";
                        return Page();
                    }
                    if (error.Code == "PasswordRequiresDigit")
                    {
                        ErrorMessage = "❌ La contraseña debe tener al menos un número.";
                        return Page();
                    }
                    if (error.Code == "PasswordRequiresUpper")
                    {
                        ErrorMessage = "❌ La contraseña debe tener al menos una mayúscula.";
                        return Page();
                    }
                    if (error.Code == "PasswordRequiresLower")
                    {
                        ErrorMessage = "❌ La contraseña debe tener al menos una minúscula.";
                        return Page();
                    }
                    ErrorMessage = $"❌ {error.Description}";
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                // ✅ 8. Capturar cualquier error inesperado
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
        /// ViewModel para el formulario de registro
        /// </summary>
        public class RegistroViewModel
        {
            [Required(ErrorMessage = "Los nombres son obligatorios")]
            [MaxLength(100)]
            public string Nombres { get; set; } = string.Empty;

            [Required(ErrorMessage = "Los apellidos son obligatorios")]
            [MaxLength(100)]
            public string Apellidos { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio")]
            [EmailAddress(ErrorMessage = "Formato de correo inválido")]
            public string Email { get; set; } = string.Empty;

            [MaxLength(20)]
            public string? Telefono { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirma tu contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}