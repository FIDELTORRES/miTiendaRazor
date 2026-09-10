using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace miTienda.Pages.Pagos
{
    public class TransferenciaModel : PageModel
    {
        public List<CarritoItem> Items { get; set; } = new();
        public decimal? Subtotal { get; set; }
        public decimal? Igv { get; set; }
        public decimal? Total { get; set; }

        [BindProperty]
        public string NumeroOperacion { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Transferencia" });
            }

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

            Subtotal = Items.Sum(i => i.Subtotal);
            Igv = Subtotal * 0.18m;
            Total = Subtotal + Igv;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Transferencia" });
            }

            if (string.IsNullOrEmpty(NumeroOperacion))
            {
                ModelState.AddModelError("NumeroOperacion", "Debes ingresar el número de operación.");
                return Page();
            }

            TempData["Success"] = "✅ ¡Transferencia confirmada!";
            return RedirectToPage("/Pagos/Exitoso");
        }
    }
}