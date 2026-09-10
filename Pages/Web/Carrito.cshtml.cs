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
using Microsoft.AspNetCore.Identity;

namespace miTienda.Pages.Web
{
    public class CarritoModel : PageModel
    {
        // ============================================================
        // 📌 CONTEXTO DE BASE DE DATOS
        // ============================================================

        private readonly MiTiendaContext _context;

        /// <summary>
        /// Constructor - Recibe el contexto por inyección de dependencias
        /// </summary>
        public CarritoModel(MiTiendaContext context)
        {
            _context = context;
        }

        // ============================================================
        // 📌 PROPIEDADES DE LA PÁGINA
        // ============================================================

        /// <summary>
        /// Lista de items en el carrito
        /// </summary>
        public List<CarritoItem> Items { get; set; } = new();

        /// <summary>
        /// Total del carrito (calculado automáticamente)
        /// </summary>
        public decimal? Total => Items?.Sum(i => i.Subtotal);

        /// <summary>
        /// Categorías para el menú desplegable
        /// </summary>
        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();

        /// <summary>
        /// Subtotal de la venta (sin IGV)
        /// </summary>
        public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;

        /// <summary>
        /// IGV (18%)
        /// </summary>
        public decimal Igv => Subtotal * 0.18m;

        /// <summary>
        /// Total con IGV incluido
        /// </summary>
        public decimal TotalConIgv => Subtotal + Igv;

        // ============================================================
        // 📌 MÉTODOS DE IDENTIFICACIÓN DE USUARIO
        // ============================================================

        /// <summary>
        /// Obtiene el identificador del usuario (logueado o anónimo)
        /// - Si está logueado: usa el ID del usuario
        /// - Si es anónimo: usa el SessionId
        /// </summary>
        private string GetUserIdentifier()
        {
            // 🔑 Si el usuario está autenticado, usar su ID (de Identity)
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Limitar a 20 caracteres (tamaño de la columna en BD)
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

            // Limitar a 20 caracteres (tamaño de la columna en BD)
            return sessionId.Length <= 20 ? sessionId : sessionId.Substring(0, 20);
        }

        /// <summary>
        /// Verifica si el usuario está logueado
        /// </summary>
        private bool IsUserLoggedIn()
        {
            return User.Identity?.IsAuthenticated == true;
        }

        // ============================================================
        // 📌 MÉTODO GET (se ejecuta al cargar la página)
        // ============================================================

        /// <summary>
        /// Método que se ejecuta al cargar la página del carrito
        /// - Carga las categorías para el menú
        /// - Carga el carrito desde la base de datos
        /// </summary>
        public async Task OnGetAsync()
        {
            await CargarCategorias();
            await CargarCarritoDesdeBD();
        }

        // ============================================================
        // 📌 CARGA DEL CARRITO DESDE BASE DE DATOS
        // ============================================================

        /// <summary>
        /// Carga el carrito desde la tabla venta_temporal
        /// Usa el identificador del usuario (UserId o SessionId)
        /// </summary>
        private async Task CargarCarritoDesdeBD()
        {
            // Obtener el identificador del usuario
            var userIdentifier = GetUserIdentifier();

            // 🔥 CARGAR DESDE LA BASE DE DATOS (FUENTE PRINCIPAL)
            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .OrderBy(v => v.FechaIngreso)
                .ToListAsync();

            if (temporales.Any())
            {
                // ✅ Convertir los registros de BD a CarritoItem
                Items = temporales.Select(v => new CarritoItem
                {
                    ProductoId = v.IdProducto ?? 0,
                    Nombre = v.Nombre ?? "Producto",
                    Imagen = v.Imagen ?? "",
                    PrecioVenta = v.PrecioVenta ?? 0,
                    Cantidad = (int)(v.Cantidad ?? 1),
                    Codbarra = v.Codbarra ?? "",
                    PrecioCompra = v.PrecioCompra ?? 0
                }).ToList();

                // ✅ ACTUALIZAR SESSION COMO CACHE (para respaldo)
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
                    // ⚠️ Si no hay en BD, intentar cargar desde Session (backup)
                    var carritoJson = HttpContext.Session.GetString("Carrito");
                    if (!string.IsNullOrEmpty(carritoJson))
                    {
                        Items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new();
                        if (Items.Any())
                        {
                            // Sincronizar Session con BD
                            await PersistirEnVentaTemporal(Items);
                        }
                    }
                }
            }

            // ✅ Actualizar datos frescos de productos (precios, nombres, imágenes)
            await ActualizarDatosProductos();
        }

        // ============================================================
        // 📌 MIGRACIÓN DE CARRITO ANÓNIMO A USUARIO LOGUEADO
        // ============================================================

        /// <summary>
        /// 🔄 Migra el carrito anónimo al usuario logueado
        /// Se ejecuta cuando un usuario anónimo inicia sesión
        /// </summary>
        private async Task MigrarCarritoAnonimoAUsuario()
        {
            // Obtener el SessionId del carrito anónimo
            var sessionId = HttpContext.Session.Id;
            var anonIdentifier = sessionId.Length <= 20 ? sessionId : sessionId.Substring(0, 20);

            // Obtener el ID del usuario logueado
            var userIdentifier = GetUserIdentifier();

            // 🔍 Buscar carrito anónimo
            var itemsAnonimos = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == anonIdentifier)
                .ToListAsync();

            if (!itemsAnonimos.Any()) return;

            // 🔍 Buscar carrito del usuario
            var itemsUsuario = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            // 📌 Si el usuario ya tiene carrito, combinamos (sumamos cantidades)
            if (itemsUsuario.Any())
            {
                foreach (var itemAnon in itemsAnonimos)
                {
                    var existing = itemsUsuario.FirstOrDefault(u => u.IdProducto == itemAnon.IdProducto);
                    if (existing != null)
                    {
                        // ✅ Sumar cantidades del mismo producto
                        existing.Cantidad = (existing.Cantidad ?? 0) + (itemAnon.Cantidad ?? 0);
                        existing.FechaModifica = DateTime.Now;
                    }
                    else
                    {
                        // ✅ Cambiar el identificador del item anónimo al usuario
                        itemAnon.UsuarioIngreso = userIdentifier;
                        itemAnon.FechaModifica = DateTime.Now;
                        _context.VentasTemporales.Update(itemAnon);
                    }
                }
            }
            else
            {
                // ✅ Si el usuario no tiene carrito, actualizar todos los items anónimos
                foreach (var item in itemsAnonimos)
                {
                    item.UsuarioIngreso = userIdentifier;
                    item.FechaModifica = DateTime.Now;
                }
                _context.VentasTemporales.UpdateRange(itemsAnonimos);
            }

            // ❌ Eliminar los items anónimos (ya migrados)
            var anonItemsToRemove = itemsAnonimos.Where(a => a.UsuarioIngreso == anonIdentifier).ToList();
            if (anonItemsToRemove.Any())
            {
                _context.VentasTemporales.RemoveRange(anonItemsToRemove);
            }

            // 💾 Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            // 🔄 Recargar el carrito con los datos migrados
            await CargarCarritoDesdeBD();
        }

        // ============================================================
        // 📌 ACTUALIZAR DATOS DE PRODUCTOS
        // ============================================================

        /// <summary>
        /// Actualiza los datos de los productos en el carrito
        /// (precios, nombres, imágenes desde la base de datos)
        /// </summary>
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
                    item.Codbarra = producto.Codbarra ?? item.Codbarra;
                    item.PrecioCompra = producto.PrecioCompra ?? item.PrecioCompra;
                }
            }
        }

        // ============================================================
        // 📌 CARGAR CATEGORÍAS PARA EL MENÚ
        // ============================================================

        /// <summary>
        /// Carga las categorías desde la base de datos para el menú desplegable
        /// </summary>
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

        // ============================================================
        // 📌 HANDLER: AGREGAR PRODUCTO AL CARRITO (AJAX)
        // ============================================================

        /// <summary>
        /// Handler para agregar productos al carrito vía AJAX
        /// Recibe el ID del producto y la cantidad
        /// </summary>
        public async Task<IActionResult> OnPostAddAsync([FromBody] AddCarritoRequest request)
        {
            try
            {
                // ✅ Validar que el request no sea nulo
                if (request == null || request.ProductoId <= 0)
                    return new JsonResult(new { success = false, message = "Datos inválidos" });

                // ✅ Obtener el identificador del usuario
                var userIdentifier = GetUserIdentifier();

                // ✅ Buscar el producto en la base de datos
                var producto = await _context.ProductosTienda
                    .Include(pt => pt.Producto)
                    .FirstOrDefaultAsync(pt => pt.IdProducto == request.ProductoId && pt.IdTienda == 2);

                if (producto == null)
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });

                // 🔥 BUSCAR EN BASE DE DATOS si el producto ya existe en el carrito
                var existing = await _context.VentasTemporales
                    .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == request.ProductoId);

                if (existing != null)
                {
                    // ✅ Actualizar cantidad si ya existe
                    existing.Cantidad = (existing.Cantidad ?? 0) + request.Cantidad;
                    existing.FechaModifica = DateTime.Now;

                    // ✅ Actualizar precio por si cambió
                    existing.PrecioVenta = producto.PrecioVenta ?? 0;
                    existing.PrecioCompra = producto.PrecioCompra ?? 0;
                }
                else
                {
                    // ✅ Crear nuevo registro en venta_temporal
                    var temporal = new VentaTemporal
                    {
                        // 📌 IDENTIFICADOR DEL USUARIO
                        UsuarioIngreso = userIdentifier,

                        // 📌 FECHAS
                        Fecha = DateTime.Now,
                        Hora = TimeOnly.FromDateTime(DateTime.Now),
                        FechaIngreso = DateTime.Now,
                        FechaModifica = DateTime.Now,

                        // 📌 DATOS DE LA EMPRESA
                        RucEmisor = "10413820532",
                        IdLocal = "2",

                        // 📌 DATOS DE LA VENTA (FIJOS PARA WEB)
                        TipoComprobante = "10",        // TICKET
                        IdTipoFactura = "0101",        // Factura
                        IdTipoValorVenta = "01",       // Gravado
                        Serie = "10",                  // Serie por defecto
                        IdTipoAfectacion = "10",       // Gravado - Operación Onerosa
                        IdCodigoDetalle = "01",        // Código de detalle
                        TipoPago = 2,                  // Contado

                        // 📌 DATOS DEL CLIENTE (VALORES POR DEFECTO)
                        IdCliente = "12345678",
                        TipoDoc = 1,

                        // 📌 DATOS DEL PRODUCTO
                        IdProducto = request.ProductoId,
                        Codbarra = producto.Codbarra ?? "",
                        Nombre = producto.Producto?.Nombre ?? "Producto",
                        Imagen = producto.Producto?.Imagen ?? "",
                        Descripcion = producto.Producto?.Descripcion ?? "",
                        PrecioVenta = producto.PrecioVenta ?? 0,
                        PrecioCompra = producto.PrecioCompra ?? 0,
                        Cantidad = request.Cantidad,
                        RutaImagen = producto.Producto?.Imagen ?? "",  // ✅ CORREGIDO: solo nombre de imagen

                        // 📌 METODO DE PAGO (se actualiza al confirmar)
                        MedioPago = 0,

                        // 📌 USUARIO
                        UsuarioModifico = userIdentifier,
                        IdUsuarioVendedor = "WEB",
                        IdVendedor = "0"
                    };
                    _context.VentasTemporales.Add(temporal);
                }

                // 💾 Guardar cambios en la base de datos
                await _context.SaveChangesAsync();

                // ✅ ACTUALIZAR SESSION CACHE
                await ActualizarCacheSession(userIdentifier);

                // 🔢 Calcular total de items en el carrito
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

        // ============================================================
        // 📌 HANDLER: ACTUALIZAR CANTIDAD
        // ============================================================

        /// <summary>
        /// Handler para actualizar la cantidad de un producto en el carrito
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync(int productoId, int cantidad)
        {
            // ✅ Validar que la cantidad sea al menos 1
            if (cantidad < 1) cantidad = 1;

            var userIdentifier = GetUserIdentifier();

            // 🔍 Buscar el item en la base de datos
            var item = await _context.VentasTemporales
                .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == productoId);

            if (item != null)
            {
                // ✅ Actualizar cantidad
                item.Cantidad = cantidad;
                item.FechaModifica = DateTime.Now;
                await _context.SaveChangesAsync();

                // ✅ Actualizar cache de Session
                await ActualizarCacheSession(userIdentifier);
            }

            return RedirectToPage();
        }

        // ============================================================
        // 📌 HANDLER: ELIMINAR ITEM
        // ============================================================

        /// <summary>
        /// Handler para eliminar un producto del carrito
        /// </summary>
        public async Task<IActionResult> OnPostRemoveAsync(int productoId)
        {
            var userIdentifier = GetUserIdentifier();

            // 🔍 Buscar el item en la base de datos
            var item = await _context.VentasTemporales
                .FirstOrDefaultAsync(v => v.UsuarioIngreso == userIdentifier && v.IdProducto == productoId);

            if (item != null)
            {
                // ❌ Eliminar el item
                _context.VentasTemporales.Remove(item);
                await _context.SaveChangesAsync();

                // ✅ Actualizar cache de Session
                await ActualizarCacheSession(userIdentifier);
            }

            return RedirectToPage();
        }

        // ============================================================
        // 📌 HANDLER: VACIAR CARRITO
        // ============================================================

        /// <summary>
        /// Handler para vaciar completamente el carrito
        /// </summary>
        public async Task<IActionResult> OnPostClearAsync()
        {
            var userIdentifier = GetUserIdentifier();

            // 🔍 Buscar todos los items del usuario
            var items = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            if (items.Any())
            {
                // ❌ Eliminar todos los items
                _context.VentasTemporales.RemoveRange(items);
                await _context.SaveChangesAsync();
            }

            // ❌ Limpiar Session
            HttpContext.Session.Remove("Carrito");
            return RedirectToPage();
        }

        // ============================================================
        // 📌 HANDLER: OBTENER CONTEO DEL CARRITO (AJAX)
        // ============================================================

        /// <summary>
        /// Handler para obtener el número total de items en el carrito vía AJAX
        /// </summary>
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

        // ============================================================
        // 📌 MÉTODOS AUXILIARES
        // ============================================================

        /// <summary>
        /// Actualiza el cache de Session desde la base de datos
        /// </summary>
        private async Task ActualizarCacheSession(string userIdentifier)
        {
            // 📥 Obtener los items desde la base de datos
            var items = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .Select(v => new CarritoItem
                {
                    ProductoId = v.IdProducto ?? 0,
                    Nombre = v.Nombre ?? "Producto",
                    Imagen = v.Imagen ?? "",
                    PrecioVenta = v.PrecioVenta ?? 0,
                    Cantidad = (int)(v.Cantidad ?? 1),
                    Codbarra = v.Codbarra ?? "",
                    PrecioCompra = v.PrecioCompra ?? 0
                })
                .ToListAsync();

            // 💾 Guardar en Session
            HttpContext.Session.SetString("Carrito", JsonSerializer.Serialize(items));
        }

        /// <summary>
        /// Persiste los items del carrito en la tabla VentasTemporales
        /// </summary>
        private async Task PersistirEnVentaTemporal(List<CarritoItem> items)
        {
            var userIdentifier = GetUserIdentifier();

            // ❌ Eliminar registros antiguos del usuario
            var antiguos = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            if (antiguos.Any())
            {
                _context.VentasTemporales.RemoveRange(antiguos);
            }

            // ✅ Insertar nuevos registros
            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    var temporal = new VentaTemporal
                    {
                        // 📌 IDENTIFICADOR DEL USUARIO
                        UsuarioIngreso = userIdentifier,

                        // 📌 FECHAS
                        Fecha = DateTime.Now,
                        Hora = TimeOnly.FromDateTime(DateTime.Now),
                        FechaIngreso = DateTime.Now,
                        FechaModifica = DateTime.Now,

                        // 📌 DATOS DE LA EMPRESA
                        RucEmisor = "10413820532",
                        IdLocal = "2",

                        // 📌 DATOS DE LA VENTA (FIJOS PARA WEB)
                        TipoComprobante = "10",        // TICKET
                        IdTipoFactura = "0101",        // Factura
                        IdTipoValorVenta = "01",       // Gravado
                        Serie = "10",                  // Serie por defecto
                        IdTipoAfectacion = "10",       // Gravado - Operación Onerosa
                        IdCodigoDetalle = "01",        // Código de detalle
                        TipoPago = 2,                  // Contado

                        // 📌 DATOS DEL CLIENTE (VALORES POR DEFECTO)
                        IdCliente = "12345678",
                        TipoDoc = 1,

                        // 📌 DATOS DEL PRODUCTO
                        IdProducto = item.ProductoId,
                        Codbarra = item.Codbarra ?? "",
                        Nombre = item.Nombre ?? "Producto",
                        Imagen = item.Imagen ?? "",
                        Descripcion = "",
                        PrecioVenta = item.PrecioVenta ?? 0,
                        PrecioCompra = item.PrecioCompra ?? 0,
                        Cantidad = item.Cantidad,
                        RutaImagen = item.Imagen ?? "",  // ✅ CORREGIDO: solo nombre de imagen

                        // 📌 USUARIO
                        UsuarioModifico = userIdentifier,
                        IdUsuarioVendedor = userIdentifier,
                        IdVendedor = "0"
                    };
                    _context.VentasTemporales.Add(temporal);
                }
            }

            // 💾 Guardar cambios
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // 📌 CLASE PARA RECIBIR DATOS DEL AJAX
        // ============================================================

        /// <summary>
        /// Clase para recibir los datos del AJAX al agregar un producto
        /// </summary>
        public class AddCarritoRequest
        {
            public int ProductoId { get; set; }
            public int Cantidad { get; set; } = 1;
        }
    }
}