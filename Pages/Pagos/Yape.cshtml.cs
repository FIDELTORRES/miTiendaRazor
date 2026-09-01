using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class YapeModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public YapeModel(MiTiendaContext context)
        {
            _context = context;
        }

        // Total del carrito
        public decimal? Total { get; set; }

        // URL del QR con monto incluido
        public string? QRUrl { get; set; }

        // Número de teléfono para Yape
        private const string NumeroYape = "989759184";

        public async Task OnGetAsync()
        {
            // Obtener carrito de la sesión
            var carritoJson = HttpContext.Session.GetString("Carrito");
            if (!string.IsNullOrEmpty(carritoJson))
            {
                var items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new();
                Total = items.Sum(i => i.Subtotal);
            }

            // ✅ Generar QR con monto incluido usando la API de goqr.me
            // Formato: https://www.yape.com.pe/pagar/976375628?monto=XX.XX&referencia=Pedido-Web
            if (Total.HasValue && Total.Value > 0)
            {
                // URL de Yape con el monto precargado
                string yapeUrl = $"https://www.yape.com.pe/pagar/{NumeroYape}?monto={Total.Value.ToString("F2").Replace(",", ".")}&referencia=Pedido-Web-{DateTime.Now.Ticks}";
                
                // Generar QR usando la API de goqr.me (gratuita y sin autenticación)
                QRUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={Uri.EscapeDataString(yapeUrl)}&bgcolor=ffffff&color=0B6299";
            }
        }

        /**
         * Procesa el pago con Yape
         */
        public async Task<IActionResult> OnPostAsync(string nroOperacion, string remitente)
        {
            try
            {
                // Validar que se haya ingresado el número de operación
                if (string.IsNullOrWhiteSpace(nroOperacion))
                {
                    TempData["Error"] = "Por favor, ingresa el número de operación Yape.";
                    return RedirectToPage();
                }

                // Obtener carrito de la sesión
                var carritoJson = HttpContext.Session.GetString("Carrito");
                if (string.IsNullOrEmpty(carritoJson))
                {
                    TempData["Error"] = "No hay productos en el carrito.";
                    return RedirectToPage("/Web/Index");
                }

                var items = JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
                if (items == null || !items.Any())
                {
                    TempData["Error"] = "El carrito está vacío.";
                    return RedirectToPage("/Web/Index");
                }

                // Validar stock disponible
                foreach (var item in items)
                {
                    var producto = await _context.ProductosTienda
                        .FirstOrDefaultAsync(pt => pt.IdProducto == item.ProductoId && pt.IdTienda == 2);
                    
                    if (producto == null || producto.Stock < item.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente para '{item.Nombre}'. Disponible: {producto?.Stock ?? 0}";
                        return RedirectToPage("/Web/Carrito");
                    }
                }

                // Calcular totales
                decimal subtotal = items.Sum(i => (i.PrecioVenta ?? 0) * i.Cantidad);
                decimal igv = subtotal * 0.18m;
                decimal total = subtotal + igv;

                // Crear pedido
                var pedido = new PedidoWeb
                {
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    FechaIngreso = DateTime.Now,
                    FechaModifica = DateTime.Now,
                    RucEmisor = "10413820532",
                    IdLocal = "2",
                    UsuarioIngreso = "web",
                    UsuarioModifico = "web",
                    TipoPago = 2,
                    EstadoComprobante = 1,
                    SubtotalVenta = subtotal,
                    Igv = igv,
                    TotalVenta = total,
                    TasaIgv = 18.00m,
                    Observacion = $"Yape - Operación: {nroOperacion} - Remitente: {remitente ?? "No especificado"}"
                };

                _context.PedidosWeb.Add(pedido);
                await _context.SaveChangesAsync();

                // Crear detalles, actualizar stock y kardex
                foreach (var item in items)
                {
                    var producto = await _context.ProductosTienda
                        .Include(p => p.Producto)
                        .FirstOrDefaultAsync(pt => pt.IdProducto == item.ProductoId && pt.IdTienda == 2);

                    if (producto == null) continue;

                    var detalle = new DetalleWeb
                    {
                        IdPedidoWeb = pedido.IdPedidoWeb,
                        IdProducto = item.ProductoId,
                        Codbarra = producto.Codbarra,
                        Cantidad = item.Cantidad,
                        PrecioVenta = item.PrecioVenta ?? 0,
                        PrecioCompra = producto.PrecioCompra ?? 0,
                        ImporteTotal = item.Subtotal ?? 0,
                        Subtotal = item.Subtotal ?? 0,
                        Descripcion = item.Nombre,
                        Fecha = DateOnly.FromDateTime(DateTime.Now),
                        FechaRegistro = DateTime.Now,
                        IdTienda = 2,
                        EstadoProducto = 1
                    };
                    _context.DetallesWeb.Add(detalle);

                    // Actualizar stock
                    producto.Stock -= item.Cantidad;
                    producto.FechaModifica = DateOnly.FromDateTime(DateTime.Now);
                    producto.HoraModifica = TimeOnly.FromDateTime(DateTime.Now);
                    _context.ProductosTienda.Update(producto);

                    // Kardex
                    var kardex = new Kardex
                    {
                        IdProducto = producto.Producto?.Codigo ?? "",
                        ProductoId = item.ProductoId,
                        Cantidad = 0,
                        CantidadSalida = item.Cantidad,
                        Fecha = DateOnly.FromDateTime(DateTime.Now),
                        Hora = TimeOnly.FromDateTime(DateTime.Now),
                        IdAlmacen = 2,
                        IdLocal = 2,
                        Estado = 2,
                        RucEmisor = "10413820532",
                        IdUsuario = "web",
                        Observacion = $"Venta YAPE #{pedido.IdPedidoWeb} - Op: {nroOperacion}",
                        PrecioUnitario = item.PrecioVenta ?? 0,
                        IdFlag = 2
                    };
                    _context.Kardex.Add(kardex);
                }

                await _context.SaveChangesAsync();

                // Registrar el pago
                var pago = new Pago
                {
                    PedidoId = pedido.IdPedidoWeb.ToString(),
                    Pasarela = "YAPE",
                    TransaccionId = nroOperacion,
                    Estado = "PENDIENTE",
                    Monto = total,
                    Moneda = "PEN",
                    FechaPago = DateTime.Now,
                    IdVenta = pedido.IdPedidoWeb
                };
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                // Limpiar carrito
                HttpContext.Session.Remove("Carrito");
                var temporales = await _context.VentasTemporales
                    .Where(v => v.UsuarioIngreso == "web")
                    .ToListAsync();
                if (temporales.Any())
                {
                    _context.VentasTemporales.RemoveRange(temporales);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "✅ Pago con Yape registrado. Tu pedido está en proceso de verificación.";
                return RedirectToPage("/Pagos/Exitoso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Yape: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al procesar el pago. Intenta nuevamente.";
                return RedirectToPage();
            }
        }
    }
}