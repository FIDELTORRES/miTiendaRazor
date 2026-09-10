using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Cuenta
{
    public class PerfilModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly UserManager<UsuarioCliente> _userManager;

        public PerfilModel(MiTiendaContext context, UserManager<UsuarioCliente> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public PerfilViewModel Perfil { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Cuenta/Perfil" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            // ✅ Cargar datos actuales del usuario
            Perfil.Nombres = usuario.Nombres ?? "";
            Perfil.Apellidos = usuario.Apellidos ?? "";
            Perfil.Email = usuario.Email ?? "";
            Perfil.Telefono = usuario.Telefono ?? "";
            Perfil.Direccion = usuario.Direccion ?? "";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
            {
                ErrorMessage = "Usuario no encontrado.";
                return Page();
            }

            try
            {
                // ✅ Actualizar datos del usuario
                usuario.Nombres = Perfil.Nombres?.Trim();
                usuario.Apellidos = Perfil.Apellidos?.Trim();
                usuario.Telefono = Perfil.Telefono?.Trim();
                usuario.Direccion = Perfil.Direccion?.Trim();

                var result = await _userManager.UpdateAsync(usuario);

                if (result.Succeeded)
                {
                    SuccessMessage = "Tus datos se actualizaron correctamente.";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ErrorMessage = error.Description;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar: {ex.Message}";
            }

            return Page();
        }

        public class PerfilViewModel
        {
            [Required(ErrorMessage = "Los nombres son obligatorios")]
            [MaxLength(100)]
            public string Nombres { get; set; } = string.Empty;

            [Required(ErrorMessage = "Los apellidos son obligatorios")]
            [MaxLength(100)]
            public string Apellidos { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            [MaxLength(20)]
            public string? Telefono { get; set; }

            [MaxLength(255)]
            public string? Direccion { get; set; }
        }
    }
}