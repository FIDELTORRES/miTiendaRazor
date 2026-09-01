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

namespace miTienda.Pages.Web
{
    public class CarritoModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public CarritoModel(MiTiendaContext context)
        {
            _context = context;
        }

        public List<CarritoItem> Items { get; set; } = new();
        public decimal? Total => Items?.Sum(i => i.Subtotal);
        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();

        // Resumen para el checkout
        public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;
        public decimal Igv => Subtotal * 0.18m;
        public decimal TotalConIgv => Subtotal + Igv;

        /// <summary>
        /// Obtiene el identificador del usuario (logueado o anónimo)
        /// </summary>
        private string GetUserIdentifier()
        {
            // 🔑 Si el usuario está autenticado, usar su ID
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

        private bool IsUserLoggedIn()
        {
            return User.Identity?.IsAuthenticated == true;
        }

        public async Task OnGetAsync()
        {
            await CargarCategorias();
            await CargarCarritoDesdeBD();
        }

        private async Task CargarCarritoDesdeBD()
        {
            var userIdentifier = GetUserIdentifier();

            // 🔥 CARGAR DESDE LA BASE DE DATOS (FUENTE PRINCIPAL)
            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .OrderBy(v => v.FechaIngreso)
                .ToListAsync();

            if (temporales.Any())
            {
                Items = temporales.Select(v => new CarritoItem
                {
                    ProductoId = v.IdProducto ?? 0,
                    Nombre = v.Nombre ?? "Producto",
                    Imagen = v.Imagen ?? "",
                    PrecioVenta = v.PrecioVenta ?? 0,
                    Cantidad = (int)(v.Cantidad ?? 1)
                }).ToList();

                // ✅ ACTUALIZAR SESSION COMO CACHE
                HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(Items));
            }
            else
            {
                // 🔄 Si el usuario está logueado, verificar si tiene carrito anónimo
                if (IsUserLoggedIn())
                {
                    await MigrarCarritoAnonimoAUsuario();
                }
                else
                {
                    // Si no hay en BD, intentar cargar desde Session (backup)
                    var carritoJson = HttpContext.Session.GetString("Carrito");
                    if (!string.IsNullOrEmpty(carritoJson))
                    {
                        Items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new();
                        if (Items.Any())
                        {
                            await PersistirEnVentaTemporal(Items);
                        }
                    }
                }
            }

            await ActualizarDatosProductos();
        }

        /// <summary>
        /// 🔄 Migra el carrito anónimo al usuario logueado
        /// </summary>
        private async Task MigrarCarritoAnonimoAUsuario()
        {
            var sessionId = HttpContext.Session.Id;
            var anonIdentifier = sessionId.Length <= 20 ? sessionId : sessionId.Substring(0, 20);
            var userIdentifier = GetUserIdentifier();

            // Buscar carrito anónimo
            var itemsAnonimos = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == anonIdentifier)
                .ToListAsync();

            if (!itemsAnonimos.Any()) return;

            // Buscar carrito del usuario
            var itemsUsuario = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            // Si el usuario ya tiene carrito, combinamos
            if (itemsUsuario.Any())
            {
                foreach (var itemAnon in itemsAnonimos)
                {
                    var existing = itemsUsuario.FirstOrDefault(u => u.IdProducto == itemAnon.IdProducto);
                    if (existing != null)
                    {
                        existing.Cantidad = (existing.Cantidad ?? 0) + (itemAnon.Cantidad ?? 0);
                        existing.FechaModifica = DateTime.Now;
                    }
                    else
                    {
                        // Cambiar el identificador del item anónimo al usuario
                        itemAnon.UsuarioIngreso = userIdentifier;
                        itemAnon.FechaModifica = DateTime.Now;
                        _context.VentasTemporales.Update(itemAnon);
                    }
                }
            }
            else
            {
                // Si el usuario no tiene carrito, actualizar todos los items anónimos
                foreach (var item in itemsAnonimos)
                {
                    item.UsuarioIngreso = userIdentifier;
                    item.FechaModifica = DateTime.Now;
                }
                _context.VentasTemporales.UpdateRange(itemsAnonimos);
            }

            // Eliminar los items anónimos (ya migrados)
            var anonItemsToRemove = itemsAnonimos.Where(a => a.UsuarioIngreso == anonIdentifier).ToList();
            if (anonItemsToRemove.Any())
            {
                _context.VentasTemporales.RemoveRange(anonItemsToRemove);
            }

            await _context.SaveChangesAsync();

            // Recargar el carrito
            await CargarCarritoDesdeBD();
        }

        private async Task ActualizarDatosProductos()
        {
            foreach (var item in Items)
            {
                var producto = await _context.ProductosTienda
                    .Include(pt => pt.Producto)
                    .FirstOrDefaultAsync(pt => pt.IdProducto == item.ProductoId && pt.IdTienda == 2);

                if (producto != null)
                {
                    item.Nombre = producto.Producto?.Nombre ?? item.Nombre;
                    item.Imagen = producto.Producto?.Imagen ?? item.Imagen;
                    item.PrecioVenta = producto.PrecioVenta ?? item.PrecioVenta;
                }
            }
        }

        private async Task CargarCategorias()
        {
            var datos = await _context.Database
                .SqlQueryRaw<CategoriaMenu>(@"
                    SELECT 
                        CAST(cp.idsubcategoria AS CHAR) AS IdSubcategoria,
                        CAST(cp.idcategoriapropia AS CHAR) AS IdCategoriapropia,
                        ct.descripcion AS CategoriaPropiaDesc,
                        CAST(cp.idcategoriapagina AS CHAR) AS IdCategoriapagina,
                        cp.descripcion AS CategoriaPaginaDesc,
                        CAST(sb.idsubcategoriapagina AS CHAR) AS IdSubcategoriapagina,
                        sb.descripcion AS SubcategoriaPaginaDesc
                    FROM categoriapagina cp
                    INNER JOIN categoriapropia ct ON cp.idcategoriapropia = ct.idcategoriapropia
                    INNER JOIN subcategoriapagina sb ON cp.idcategoriapagina = sb.idcategoriapagina
                    ORDER BY ct.descripcion ASC, cp.descripcion ASC, sb.descripcion ASC
                ")
                .ToListAsync();

            CategoriasNivel1 = datos
                .GroupBy(c => new { c.IdCategoriapropia, c.CategoriaPropiaDesc })
                .Select(g1 => new CategoriaNivel1
                {
                    IdCategoriapropia = g1.Key.IdCategoriapropia,
                    CategoriaPropiaDesc = g1.Key.CategoriaPropiaDesc,
                    Nivel2 = g1
                        .GroupBy(c => new { c.IdCategoriapagina, c.CategoriaPaginaDesc })
                        .Select(g2 => new CategoriaNivel2
                        {
                            IdCategoriapagina = g2.Key.IdCategoriapagina,
                            CategoriaPaginaDesc = g2.Key.CategoriaPaginaDesc,
                            Nivel3 = g2.Select(c => new CategoriaNivel3
                            {
                                IdSubcategoria = c.IdSubcategoria,
                                SubcategoriaPaginaDesc = c.SubcategoriaPaginaDesc
                            }).ToList()
                        }).ToList()
                }).ToList();

            ViewData["CategoriasNivel1"] = CategoriasNivel1;
        }

        // ✅ HANDLER: Agregar al carrito (AJAX)
        public async Task<IActionResult> OnPostAddAsync([FromBody] AddCarritoRequest request)
        {
            try
            {
                if (request == null || request.ProductoId <= 0)
                    return new JsonResult(new { success = false, message = "Datos inválidos" });

                var userIdentifier = GetUserIdentifier();

                var producto = await _context.ProductosTienda
                    .Include(pt => pt.Producto)
                    .FirstOrDefaultAsync(pt => pt.IdProducto == request.ProductoId && pt.IdTienda == 2);

                if (producto == null)
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });

                // 🔥 BUSCAR EN BASE DE DATOS
                var existing = await _context.VentasTemporales
                    .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == request.ProductoId);

                if (existing != null)
                {
                    existing.Cantidad = (existing.Cantidad ?? 0) + request.Cantidad;
                    existing.FechaModifica = DateTime.Now;
                }
                else
                {
                    var temporal = new VentaTemporal
                    {
                        UsuarioIngreso = userIdentifier,
                        IdProducto = request.ProductoId,
                        Nombre = producto.Producto?.Nombre ?? "Producto",
                        Imagen = producto.Producto?.Imagen ?? "",
                        PrecioVenta = producto.PrecioVenta ?? 0,
                        Cantidad = request.Cantidad,
                        Fecha = DateTime.Now,
                        FechaIngreso = DateTime.Now,
                        FechaModifica = DateTime.Now,
                        Hora = TimeOnly.FromDateTime(DateTime.Now),
                        TipoPago = 2,
                        RucEmisor = "10413820532",
                        IdLocal = "2",
                        UsuarioModifico = userIdentifier
                    };
                    _context.VentasTemporales.Add(temporal);
                }

                await _context.SaveChangesAsync();

                // ✅ ACTUALIZAR SESSION CACHE
                await ActualizarCacheSession(userIdentifier);

                var totalItems = await _context.VentasTemporales
                    .Where(v => v.UsuarioIngreso == userIdentifier)
                    .SumAsync(v => (int?)v.Cantidad) ?? 0;

                return new JsonResult(new
                {
                    success = true,
                    totalItems = totalItems,
                    message = "Producto agregado correctamente"
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        // ✅ HANDLER: Actualizar cantidad
        public async Task<IActionResult> OnPostUpdateAsync(int productoId, int cantidad)
        {
            if (cantidad < 1) cantidad = 1;
            var userIdentifier = GetUserIdentifier();

            var item = await _context.VentasTemporales
                .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == productoId);

            if (item != null)
            {
                item.Cantidad = cantidad;
                item.FechaModifica = DateTime.Now;
                await _context.SaveChangesAsync();
                await ActualizarCacheSession(userIdentifier);
            }

            return RedirectToPage();
        }

        // ✅ HANDLER: Eliminar item
        public async Task<IActionResult> OnPostRemoveAsync(int productoId)
        {
            var userIdentifier = GetUserIdentifier();

            var item = await _context.VentasTemporales
                .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == productoId);

            if (item != null)
            {
                _context.VentasTemporales.Remove(item);
                await _context.SaveChangesAsync();
                await ActualizarCacheSession(userIdentifier);
            }

            return RedirectToPage();
        }

        // ✅ HANDLER: Vaciar carrito
        public async Task<IActionResult> OnPostClearAsync()
        {
            var userIdentifier = GetUserIdentifier();

            var items = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            if (items.Any())
            {
                _context.VentasTemporales.RemoveRange(items);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove("Carrito");
            return RedirectToPage();
        }

        // ✅ Obtener conteo del carrito (AJAX)
        public async Task<IActionResult> OnGetCount()
        {
            try
            {
                var userIdentifier = GetUserIdentifier();
                var totalItems = await _context.VentasTemporales
                    .Where(v => v.UsuarioIngreso == userIdentifier)
                    .SumAsync(v => (int?)v.Cantidad) ?? 0;

                return new JsonResult(new { totalItems });
            }
            catch
            {
                return new JsonResult(new { totalItems = 0 });
            }
        }

        // ✅ Métodos auxiliares
        private async Task ActualizarCacheSession(string userIdentifier)
        {
            var items = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .Select(v => new CarritoItem
                {
                    ProductoId = v.IdProducto ?? 0,
                    Nombre = v.Nombre ?? "Producto",
                    Imagen = v.Imagen ?? "",
                    PrecioVenta = v.PrecioVenta ?? 0,
                    Cantidad = (int)(v.Cantidad ?? 1)
                })
                .ToListAsync();

            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(items));
        }

        private async Task PersistirEnVentaTemporal(List<CarritoItem> items)
        {
            var userIdentifier = GetUserIdentifier();

            var antiguos = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            if (antiguos.Any())
            {
                _context.VentasTemporales.RemoveRange(antiguos);
            }

            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    var temporal = new VentaTemporal
                    {
                        UsuarioIngreso = userIdentifier,
                        IdProducto = item.ProductoId,
                        Nombre = item.Nombre ?? "Producto",
                        Imagen = item.Imagen ?? "",
                        PrecioVenta = item.PrecioVenta ?? 0,
                        Cantidad = item.Cantidad,
                        Fecha = DateTime.Now,
                        FechaIngreso = DateTime.Now,
                        FechaModifica = DateTime.Now,
                        Hora = TimeOnly.FromDateTime(DateTime.Now),
                        TipoPago = 2,
                        RucEmisor = "10413820532",
                        IdLocal = "2",
                        UsuarioModifico = userIdentifier
                    };
                    _context.VentasTemporales.Add(temporal);
                }
            }

            await _context.SaveChangesAsync();
        }

        public class AddCarritoRequest
        {
            public int ProductoId { get; set; }
            public int Cantidad { get; set; } = 1;
        }
    }
}