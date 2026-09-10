using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Cuenta
{
    public class SeguridadModel : PageModel
    {
        private readonly UserManager<UsuarioCliente> _userManager;

        // Constructor recibe UserManager<UsuarioCliente> como parámetro y lo asigna a la variable privada _userManager
        public SeguridadModel(UserManager<UsuarioCliente> userManager)
        {
            _userManager = userManager;
        }
        // [BindProperty] enlaza la propiedad CambiarPassword con los datos enviados por el usuario desde el formulario HTML en la vista.
        [BindProperty]
        public CambiarPasswordViewModel CambiarPassword { get; set; } = new();
        // variables para almacenar los mensajes que se le mostrarán al usuario.
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        
        //OnGet() se ejecuta de forma automática cuando el usuario accede a la página mediante una petición HTTP GET (al abrir la URL en el navegador).  Actualmente está vacío porque solo se necesita mostrar el formulario limpio sin cargar datos previos de la base de datos.
        public void OnGet()
        {
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

            // ✅ Cambiar la contraseña
            var result = await _userManager.ChangePasswordAsync(
                usuario,
                CambiarPassword.PasswordActual,
                CambiarPassword.NuevaPassword);

            if (result.Succeeded)
            {
                SuccessMessage = "Tu contraseña se cambió correctamente.";
                CambiarPassword = new CambiarPasswordViewModel();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        ErrorMessage = "La contraseña actual es incorrecta.";
                        return Page();
                    }
                    ErrorMessage = error.Description;
                    break;
                }
            }

            return Page();
        }

        public class CambiarPasswordViewModel
        {
            [Required(ErrorMessage = "La contraseña actual es obligatoria")]
            [DataType(DataType.Password)]
            public string PasswordActual { get; set; } = string.Empty;

            [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
            [DataType(DataType.Password)]
            public string NuevaPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirma la nueva contraseña")]
            [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
            [DataType(DataType.Password)]
            public string ConfirmarPassword { get; set; } = string.Empty;
        }
    }
}