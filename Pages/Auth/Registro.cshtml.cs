using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace miTienda.Pages.Auth
{
    public class RegistroModel : PageModel
    {
        private readonly UserManager<UsuarioCliente> _userManager;
        private readonly SignInManager<UsuarioCliente> _signInManager;
        private readonly MiTiendaContext _context;

        public RegistroModel(
            UserManager<UsuarioCliente> userManager,
            SignInManager<UsuarioCliente> signInManager,
            MiTiendaContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public RegistroViewModel Registro { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public bool EmailExists { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";
            EmailExists = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // ✅ Verificar si el email ya existe en `usuario_cliente`
                var existingUser = await _userManager.FindByEmailAsync(Registro.Email.Trim());
                if (existingUser != null)
                {
                    EmailExists = true;
                    ErrorMessage = "❌ Este correo electrónico ya está registrado.";
                    return Page();
                }

                // ============================================================
                // 📌 CREAR USUARIO EN `usuario_cliente`
                // ============================================================

                var usuario = new UsuarioCliente
                {
                    UserName = Registro.Email.Trim(),
                    Email = Registro.Email.Trim(),
                    Nombres = Registro.Nombres?.Trim(),
                    Apellidos = Registro.Apellidos?.Trim(),
                    Telefono = Registro.Telefono?.Trim() ?? "",
                    FechaRegistro = DateTime.Now,
                    Estado = 1,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                var result = await _userManager.CreateAsync(usuario, Registro.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "DuplicateEmail" || error.Code == "DuplicateUserName")
                        {
                            EmailExists = true;
                            ErrorMessage = "❌ Este correo electrónico ya está registrado.";
                            return Page();
                        }
                        ErrorMessage = $"❌ {error.Description}";
                        return Page();
                    }
                    return Page();
                }

                // ✅ Asignar rol y loguear
                try
                {
                    await _userManager.AddToRoleAsync(usuario, "Cliente");
                }
                catch { }

                await _signInManager.SignInAsync(usuario, isPersistent: false);

                // ✅ Redirigir al flujo de pago
                return RedirectToLocal(ReturnUrl);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ Error inesperado: {ex.Message}";
                return Page();
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToPage("/Web/Index");
        }

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