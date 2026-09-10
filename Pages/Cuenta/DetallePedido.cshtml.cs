using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Cuenta
{
    public class DetallePedidoModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly UserManager<UsuarioCliente> _userManager;

        public DetallePedidoModel(MiTiendaContext context, UserManager<UsuarioCliente> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Pedido a mostrar
        public PedidoWeb? Pedido { get; set; }

        // ✅ Lista de detalles del pedido
        public List<DetalleWeb> Detalles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // ✅ Verificar que el usuario esté logueado
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = $"/Cuenta/DetallePedido/{id}" });
            }

            // ✅ Obtener identificador del usuario
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userIdentifier = userId.Length <= 20 ? userId : userId.Substring(0, 20);

            // ✅ Buscar el pedido (solo si pertenece al usuario logueado)
            Pedido = await _context.PedidosWeb
                .FirstOrDefaultAsync(p => p.IdPedidoWeb == id && p.UsuarioIngreso == userIdentifier);

            if (Pedido == null)
            {
                // Pedido no encontrado o no pertenece al usuario
                return Page();
            }

            // ✅ Cargar los detalles del pedido
            Detalles = await _context.DetallesWeb
                .Where(d => d.IdPedidoWeb == id)
                .ToListAsync();

            return Page();
        }
    }
}