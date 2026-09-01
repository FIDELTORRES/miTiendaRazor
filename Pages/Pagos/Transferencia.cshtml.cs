using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;  // ← Asegúrate de que sea el espacio de nombres correcto
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class TransferenciaModel : PageModel
    {
        private readonly MiTiendaContext _context;  // ← Solo uso, no definición

        public TransferenciaModel(MiTiendaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(string fecha, string hora, string nroTransaccion, decimal monto, string remitente)
        {
            // 1. Obtener carrito
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

            // 2. Validar stock
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

            // 3. Crear cabecera de pedido (pedidoweb)
            decimal subtotal = items.Sum(i => (i.PrecioVenta ?? 0) * i.Cantidad);
            decimal igv = subtotal * 0.18m;
            decimal total = subtotal + igv;

            var pedido = new PedidoWeb
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Hora = TimeOnly.FromDateTime(DateTime.Now),
                FechaIngreso = DateTime.Now,
                FechaModifica = DateTime.Now,
                RucEmisor = "10413820532",
                IdLocal = "2",
                UsuarioIngreso = "Web",
                UsuarioModifico = "Web",
                TipoPago = 2,
                EstadoComprobante = 1,
                SubtotalVenta = subtotal,
                Igv = igv,
                TotalVenta = total,
                TasaIgv = 18.00m
            };

            _context.PedidosWeb.Add(pedido);
            await _context.SaveChangesAsync();

            // 4. Detalle, actualizar stock y kardex
            foreach (var item in items)
            {
                var producto = await _context.ProductosTienda
                    .Include(p => p.Producto)
                    .FirstOrDefaultAsync(pt => pt.IdProducto == item.ProductoId && pt.IdTienda == 2);

                if (producto == null) continue;

                // Detalle
                var detalle = new DetalleWeb
                {
                    IdPedidoWeb = pedido.IdPedidoWeb,
                    IdProducto = item.ProductoId,
                    Codbarra = producto.Codbarra,
                    Cantidad = item.Cantidad,
                    PrecioVenta = item.PrecioVenta,
                    PrecioCompra = producto.PrecioCompra ?? 0,
                    ImporteTotal = item.Subtotal,
                    Subtotal = item.Subtotal,
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

                // Kardex (salida por venta)
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
                    RucEmisor = "10413820532",  // RUC de la empresa 
                    IdUsuario = "Web",
                    Observacion = $"Venta #{pedido.IdPedidoWeb}",
                    PrecioUnitario = item.PrecioVenta,
                    IdFlag = 2
                };
                _context.Kardex.Add(kardex);
            }

            await _context.SaveChangesAsync();

            // 5. Limpiar carrito
            HttpContext.Session.Remove("Carrito");
            string sessionId = HttpContext.Session.Id;
            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == sessionId)
                .ToListAsync();
            if (temporales.Any())
            {
                _context.VentasTemporales.RemoveRange(temporales);
                await _context.SaveChangesAsync();
            }

            // 6. Registrar pago
            var pago = new Pago
            {
                PedidoId = pedido.IdPedidoWeb.ToString(),
                Pasarela = "TRANSFERENCIA",
                TransaccionId = nroTransaccion,
                Estado = "APROBADO",
                Monto = total,
                Moneda = "PEN",
                FechaPago = DateTime.Now,
                IdVenta = pedido.IdPedidoWeb
            };
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Pago registrado exitosamente. ¡Gracias por tu compra!";
            return RedirectToPage("/Pagos/Exitoso");
        }
    }
}