using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace miTienda.Pages.Pagos
{
    public class PlinModel : PageModel
    {
        public List<CarritoItem> Items { get; set; } = new();
        public decimal? Subtotal { get; set; }
        public decimal? Igv { get; set; }
        public decimal? Total { get; set; }

        [BindProperty]
        public string CodigoConfirmacion { get; set; } = string.Empty;

        public string CodigoTransaccion { get; private set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Plin" });
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

            CodigoTransaccion = $"PLIN-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Plin" });
            }

            if (string.IsNullOrEmpty(CodigoConfirmacion))
            {
                ModelState.AddModelError("CodigoConfirmacion", "Debes ingresar el código de transacción.");
                return Page();
            }

            TempData["Success"] = "✅ ¡Pago con Plin confirmado!";
            return RedirectToPage("/Pagos/Exitoso");
        }
    }
}