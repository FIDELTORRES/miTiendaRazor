using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace miTienda.Pages.Pagos
{
    public class EfectivoModel : PageModel
    {
        public List<CarritoItem> Items { get; set; } = new();
        public decimal? Subtotal { get; set; }
        public decimal? Igv { get; set; }
        public decimal? Total { get; set; }

        [BindProperty]
        public PedidoEfectivo Pedido { get; set; } = new();

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Efectivo" });
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
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Efectivo" });
            }

            if (string.IsNullOrEmpty(Pedido.Nombre) || string.IsNullOrEmpty(Pedido.Direccion))
            {
                ModelState.AddModelError("", "Completa todos los campos obligatorios.");
                return Page();
            }

            // Aquí guardarías el pedido en la base de datos
            TempData["Success"] = "✅ ¡Pedido confirmado! Pagarás al recibir.";
            return RedirectToPage("/Pagos/Exitoso");
        }

        public class PedidoEfectivo
        {
            public string Nombre { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string Indicaciones { get; set; } = string.Empty;
        }
    }
}