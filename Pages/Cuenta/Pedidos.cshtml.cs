using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Cuenta
{
    public class PedidosModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly UserManager<UsuarioCliente> _userManager;

        public PedidosModel(MiTiendaContext context, UserManager<UsuarioCliente> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PedidoWeb> Pedidos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string EstadoFiltro { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // ✅ Verificar que el usuario esté logueado
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Cuenta/Pedidos" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            // ✅ Buscar pedidos por el identificador del usuario (usuarioingreso)
            var userIdentifier = userId.Length <= 20 ? userId : userId.Substring(0, 20);

            var query = _context.PedidosWeb
                .Where(p => p.UsuarioIngreso == userIdentifier)
                .AsQueryable();

            // ✅ Aplicar filtro por estado
            if (!string.IsNullOrEmpty(EstadoFiltro) && int.TryParse(EstadoFiltro, out int estado))
            {
                query = query.Where(p => p.EstadoPedido == estado);
            }

            // ✅ Ordenar por fecha descendente (más recientes primero)
            Pedidos = await query
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.Hora)
                .ToListAsync();

            return Page();
        }
    }
}