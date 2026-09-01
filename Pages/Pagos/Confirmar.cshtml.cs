using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class ConfirmarModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public ConfirmarModel(MiTiendaContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Metodo { get; set; } = string.Empty;

        [BindProperty]
        public PedidoWeb Pedido { get; set; } = new();

        public List<CarritoItem> Items { get; set; } = new();
        public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;
        public decimal Igv => Subtotal * 0.18m;
        public decimal TotalConIgv => Subtotal + Igv;

        public string MetodoPagoDisplay => Metodo switch
        {
            "yape" => "Yape",
            "plin" => "Plin",
            "transferencia" => "Transferencia Bancaria",
            "efectivo" => "Efectivo (contraentrega)",
            _ => Metodo
        };

        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();

        private string GetUserIdentifier()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId.Length <= 20 ? userId : userId.Substring(0, 20);
                }
            }
            return HttpContext.Session.Id.Length <= 20 ? HttpContext.Session.Id : HttpContext.Session.Id.Substring(0, 20);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarCategorias();

            // 🔒 Verificar que el usuario esté logueado
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = $"/Pagos/Confirmar?metodo={Metodo}" });
            }

            // Verificar que haya un método seleccionado
            if (string.IsNullOrEmpty(Metodo))
            {
                return RedirectToPage("/Pagos/Index");
            }

            // Cargar carrito
            await CargarCarrito();

            if (!Items.Any())
            {
                return RedirectToPage("/Web/Carrito");
            }

            // Pre-cargar datos del usuario si existe
            var userIdentifier = GetUserIdentifier();
            // Aquí puedes cargar datos del usuario desde tu tabla de usuarios
            // Por ahora, dejamos campos vacíos

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarCarrito();
                return Page();
            }

            var userIdentifier = GetUserIdentifier();

            // 1. Crear el pedido
            Pedido.Fecha = DateOnly.FromDateTime(DateTime.Now);
            Pedido.Hora = TimeOnly.FromDateTime(DateTime.Now);
            Pedido.FechaIngreso = DateTime.Now;
            Pedido.FechaModifica = DateTime.Now;
            Pedido.UsuarioIngreso = userIdentifier;
            Pedido.UsuarioModifico = userIdentifier;
            Pedido.RucEmisor = "10413820532";
            Pedido.IdLocal = "2";
            Pedido.TipoPago = Metodo switch
            {
                "efectivo" => 1, // Contado
                _ => 2 // Crédito/Digital
            };
            Pedido.TipoEntrega = 1; // Delivery
            Pedido.EstadoPedido = 1; // Pendiente

            // Calcular totales desde el carrito
            await CargarCarrito();
            Pedido.SubtotalVenta = Subtotal;
            Pedido.Igv = Igv;
            Pedido.TotalVenta = TotalConIgv;

            // _context.PedidoWebs.Add(Pedido);
            _context.PedidosWeb.Add(Pedido);
            await _context.SaveChangesAsync();

            // 2. Crear los detalles del pedido
            foreach (var item in Items)
            {
                var detalle = new DetalleWeb
                {
                    IdPedidoWeb = Pedido.IdPedidoWeb,
                    IdProducto = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioVenta = item.PrecioVenta,
                    ImporteTotal = item.Subtotal,
                    Descripcion = item.Nombre,
                    FechaRegistro = DateTime.Now,
                    IdTienda = 2,
                    DirImagen = item.Imagen ?? ""
                };
                _context.DetallesWeb.Add(detalle);
            }
            await _context.SaveChangesAsync();

            // 3. Limpiar carrito temporal
            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();
            if (temporales.Any())
            {
                _context.VentasTemporales.RemoveRange(temporales);
                await _context.SaveChangesAsync();
            }

            // 4. Limpiar Session
            HttpContext.Session.Remove("Carrito");

            // 5. Redirigir a la página de éxito
            return RedirectToPage("/Pagos/Exito", new { id = Pedido.IdPedidoWeb });
        }

        private async Task CargarCarrito()
        {
            var userIdentifier = GetUserIdentifier();

            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .ToListAsync();

            Items = temporales.Select(v => new CarritoItem
            {
                ProductoId = v.IdProducto ?? 0,
                Nombre = v.Nombre ?? "Producto",
                Imagen = v.Imagen ?? "",
                PrecioVenta = v.PrecioVenta ?? 0,
                Cantidad = (int)(v.Cantidad ?? 1)
            }).ToList();
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
    }
}