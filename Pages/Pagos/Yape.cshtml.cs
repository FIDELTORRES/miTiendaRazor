using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace miTienda.Pages.Pagos
{
    public class YapeModel : PageModel
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
            // Verificar que el usuario esté logueado
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Yape" });
            }

            // Cargar carrito
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
            Igv = Subtotal * 0.18m;
            Total = Subtotal + Igv;

            // Generar código de transacción
            CodigoTransaccion = $"YAPE-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar que el usuario esté logueado
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = "/Pagos/Yape" });
            }

            // Validar código de confirmación
            if (string.IsNullOrEmpty(CodigoConfirmacion))
            {
                ModelState.AddModelError("CodigoConfirmacion", "Debes ingresar el código de transacción.");
                return Page();
            }

            // Aquí procesarías el pago y guardarías el pedido
            // Por ahora, redirigir a éxito
            TempData["Success"] = "✅ ¡Pago con Yape confirmado!";
            return RedirectToPage("/Pagos/Exitoso");
        }
    }
}