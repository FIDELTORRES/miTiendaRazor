using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class IndexModel : PageModel
    {
        // Items del carrito
        public List<CarritoItem> Items { get; set; } = new();

        // Totales
        public decimal? Subtotal { get; set; }
        public decimal? Igv { get; set; }
        public decimal? Total { get; set; }

        public IActionResult OnGet()
        {
            // Obtener carrito de la sesión
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(carritoJson))
            {
                TempData["Error"] = "No hay productos en el carrito.";
                return RedirectToPage("/Web/Index");
            }

            Items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new();

            if (!Items.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToPage("/Web/Index");
            }

            // Calcular totales
            Subtotal = Items.Sum(i => i.Subtotal);
            Igv = Subtotal * 0.18m; // 18% IGV
            Total = Subtotal + Igv;

            return Page();
        }
    }
}