using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Cuenta
{
    public class DireccionesModel : PageModel
    {
        private readonly UserManager<UsuarioCliente> _userManager;

        public DireccionesModel(UserManager<UsuarioCliente> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        [MaxLength(255)]
        public string Direccion { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Cuenta/Direcciones" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            Direccion = usuario.Direccion ?? "";
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
                return RedirectToPage("/Auth/Login");
            }

            usuario.Direccion = Direccion.Trim();
            await _userManager.UpdateAsync(usuario);

            SuccessMessage = "Tu dirección se actualizó correctamente.";
            return Page();
        }
    }
}