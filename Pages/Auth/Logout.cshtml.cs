using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System.Threading.Tasks;

namespace miTienda.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<UsuarioCliente> _signInManager;

        public LogoutModel(SignInManager<UsuarioCliente> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Web/Index");
        }
    }
}