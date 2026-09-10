using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class IndexModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public IndexModel(MiTiendaContext context)
        {
            _context = context;
        }

        // ============================================================
        // 📌 PROPIEDADES
        // ============================================================

        public List<CarritoItem> Items { get; set; } = new();
        public decimal? Subtotal { get; set; }
        public decimal? Igv { get; set; }
        public decimal? Total { get; set; }

        public decimal TotalConIgv => (Subtotal ?? 0m) + (Igv ?? 0m);

        // ============================================================
        // 📌 MÉTODO PARA OBTENER IDENTIFICADOR DEL USUARIO
        // ============================================================

        /// <summary>
        /// Obtiene el identificador del usuario (logueado o anónimo)
        /// - Si está logueado: usa el ID del usuario
        /// - Si es anónimo: usa el SessionId
        /// Siempre devuelve máximo 20 caracteres
        /// </summary>
        private string GetUserIdentifier()
        {
            // 🔑 Si el usuario está autenticado, usar su ID (de Identity)
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId.Length <= 20 ? userId : userId.Substring(0, 20);
                }
            }

            // 🔑 Si no está autenticado, usar SessionId
            var sessionId = HttpContext.Session.Id;
            if (string.IsNullOrEmpty(sessionId))
            {
                HttpContext.Session.SetString("_Init", "1");
                sessionId = HttpContext.Session.Id;
            }

            return sessionId.Length <= 20 ? sessionId : sessionId.Substring(0, 20);
        }

        // ============================================================
        // 📌 MÉTODO GET
        // ============================================================

        public async Task<IActionResult> OnGet()
        {
            // ✅ Obtener identificador del usuario
            var userIdentifier = GetUserIdentifier();

            // ✅ 1. Intentar cargar desde la base de datos
            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .ToListAsync();

            if (temporales.Any())
            {
                Items = temporales.Select(v => new CarritoItem
                {
                    ProductoId = v.IdProducto ?? 0,
                    Nombre = v.Nombre ?? "Producto",
                    Imagen = v.Imagen ?? "",
                    PrecioVenta = v.PrecioVenta ?? 0,
                    Cantidad = (int)(v.Cantidad ?? 1),
                    Codbarra = v.Codbarra ?? ""
                }).ToList();

                // ✅ Actualizar sesión con los datos de la BD
                HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(Items));
            }
            else
            {
                // ✅ 2. Si no hay en BD, intentar cargar desde sesión
                var carritoJson = HttpContext.Session.GetString("Carrito");
                if (!string.IsNullOrEmpty(carritoJson))
                {
                    Items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new();
                }
            }

            // ✅ 3. Si aún no hay items, redirigir al carrito
            if (!Items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToPage("/Web/Carrito");
            }

            // ✅ 4. Calcular totales
            Subtotal = Items.Sum(i => i.Subtotal);      // Total con IGV
            Igv = Subtotal / 1.18m * 0.18m;             // IGV = total / 1.18 * 0.18
            Total = Subtotal;                          // El total es el subtotal (ya incluye IGV)

            return Page();
        }
    }
}