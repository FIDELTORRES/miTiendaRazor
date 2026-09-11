using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using miTienda.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace miTienda.Pages.Pagos
{
    public class ConfirmarModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly UserManager<UsuarioCliente> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;   // ✅ AGREGAR

        public ConfirmarModel(
            MiTiendaContext context,
            UserManager<UsuarioCliente> userManager,
            IEmailService emailService,
            IWhatsAppService whatsAppService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
        }

        // ============================================================
        // 📌 PROPIEDADES BIND
        // ============================================================

        [BindProperty(SupportsGet = true)]
        public string Metodo { get; set; } = string.Empty;

        [BindProperty]
        public string CodigoConfirmacion { get; set; } = string.Empty;

        [BindProperty]
        public PedidoEnvio Pedido { get; set; } = new();

        // ============================================================
        // 📌 PROPIEDADES DE LA PÁGINA
        // ============================================================

        public List<CarritoItem> Items { get; set; } = new();
        public List<VentaTemporal> Temporales { get; set; } = new();
        public List<HorarioEntrega> Horarios { get; set; } = new();

        // ✅ DESPUÉS
        public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;      // Total con IGV
        public decimal Igv => Subtotal / 1.18m * 0.18m;                   // IGV = total / 1.18 * 0.18
        public decimal TotalConIgv => Subtotal;                            // Total = Subtotal

        public string MetodoDisplay => Metodo switch
        {
            "yape" => "Yape",
            "plin" => "Plin",
            "transferencia" => "Transferencia Bancaria",
            "mercadopago" => "Mercado Pago",
            _ => Metodo
        };

        public string CodigoTransaccion => Metodo switch
        {
            "yape" => $"YAPE-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            "plin" => $"PLIN-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            "transferencia" => $"TRF-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            "mercadopago" => $"MP-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            _ => ""
        };

        public int MediopagoCode => Metodo switch
        {
            "yape" => 1,
            "plin" => 2,
            "transferencia" => 3,
            "mercadopago" => 4,
            _ => 0
        };

        public string NombresCliente { get; set; } = string.Empty;
        public string ApellidosCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string DireccionCliente { get; set; } = string.Empty;

        // ============================================================
        // 📌 MÉTODO PARA OBTENER IDENTIFICADOR DEL USUARIO
        // ============================================================

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

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = $"/Pagos/Confirmar?metodo={Metodo}" });
            }

            if (string.IsNullOrEmpty(Metodo))
            {
                return RedirectToPage("/Pagos/Index");
            }

            Horarios = await _context.HorariosEntrega
                .Where(h => h.Estado == 1)
                .OrderBy(h => h.HoraEntrega)
                .ToListAsync();

            var userIdentifier = GetUserIdentifier();

            Temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .ToListAsync();

            if (!Temporales.Any())
            {
                TempData["Error"] = "No hay productos en el carrito.";
                return RedirectToPage("/Web/Index");
            }

            Items = Temporales.Select(v => new CarritoItem
            {
                ProductoId = v.IdProducto ?? 0,
                Nombre = v.Nombre ?? "Producto",
                Imagen = v.Imagen ?? "",
                PrecioVenta = v.PrecioVenta ?? 0,
                Cantidad = (int)(v.Cantidad ?? 1),
                Codbarra = v.Codbarra ?? ""
            }).ToList();

            // Cargar datos del cliente
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var usuario = await _userManager.FindByIdAsync(userId);
                if (usuario != null)
                {
                    NombresCliente = usuario.Nombres ?? "";
                    ApellidosCliente = usuario.Apellidos ?? "";
                    TelefonoCliente = usuario.Telefono ?? "";
                    DireccionCliente = usuario.Direccion ?? "";

                    if (string.IsNullOrEmpty(DireccionCliente) && usuario.IdCliente.HasValue)
                    {
                        var cliente = await _context.Clientes
                            .FirstOrDefaultAsync(c => c.IdCliente == usuario.IdCliente.Value);
                        if (cliente != null)
                        {
                            DireccionCliente = cliente.Direccion ?? "";
                            if (string.IsNullOrEmpty(NombresCliente)) NombresCliente = cliente.Nombre ?? "";
                            if (string.IsNullOrEmpty(ApellidosCliente)) ApellidosCliente = cliente.Apellidos ?? "";
                            if (string.IsNullOrEmpty(TelefonoCliente)) TelefonoCliente = cliente.Telefono ?? "";
                        }
                    }
                }
            }

            Pedido.FechaEntrega = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            return Page();
        }

        // ============================================================
        // 📌 MÉTODO POST (CORREGIDO)
        // ============================================================

        public async Task<IActionResult> OnPostAsync()
        {
            // ============================================================
            // 📌 PASO 1: VALIDACIONES
            // ============================================================

            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Auth/Login", new { returnUrl = $"/Pagos/Confirmar?metodo={Metodo}" });
            }

            var userIdentifier = GetUserIdentifier();

            if (string.IsNullOrEmpty(Pedido.DireccionEntrega))
            {
                ModelState.AddModelError("Pedido.DireccionEntrega", "La dirección de entrega es obligatoria.");
                await CargarDatosPagina();
                return Page();
            }

            if (Pedido.FechaEntrega == null)
            {
                ModelState.AddModelError("Pedido.FechaEntrega", "La fecha de entrega es obligatoria.");
                await CargarDatosPagina();
                return Page();
            }

            if (string.IsNullOrEmpty(Pedido.HoraEntrega))
            {
                ModelState.AddModelError("Pedido.HoraEntrega", "La hora de entrega es obligatoria.");
                await CargarDatosPagina();
                return Page();
            }

            if (Metodo == "yape" || Metodo == "plin" || Metodo == "transferencia")
            {
                if (string.IsNullOrEmpty(CodigoConfirmacion))
                {
                    ModelState.AddModelError("CodigoConfirmacion", "Debes ingresar el código de transacción.");
                    await CargarDatosPagina();
                    return Page();
                }
            }

            Temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .ToListAsync();

            if (!Temporales.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToPage("/Web/Carrito");
            }

            // ✅ CORREGIDO: Asignar Items con los datos del carrito ANTES de limpiar
            Items = Temporales.Select(v => new CarritoItem
            {
                ProductoId = v.IdProducto ?? 0,
                Nombre = v.Nombre ?? "Producto",
                Imagen = v.Imagen ?? "",
                PrecioVenta = v.PrecioVenta ?? 0,
                Cantidad = (int)(v.Cantidad ?? 1),
                Codbarra = v.Codbarra ?? ""
            }).ToList();

            // ============================================================
            // 📌 PASO 2: OBTENER USUARIO
            // ============================================================

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] = "No se pudo identificar al usuario.";
                return RedirectToPage("/Auth/Login");
            }

            var usuario = await _userManager.FindByIdAsync(userId);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToPage("/Auth/Login");
            }

            // ============================================================
            // 📌 PASO 3: CREAR PEDIDO
            // ============================================================

            var codigoTransaccion = string.IsNullOrEmpty(CodigoConfirmacion) 
                ? CodigoTransaccion 
                : CodigoConfirmacion;

            // ✅ DESPUÉS
            var subtotal = Temporales.Sum(v => (v.PrecioVenta ?? 0) * (v.Cantidad ?? 0)); // Total con IGV
            var igv = subtotal / 1.18m * 0.18m;  // IGV = total / 1.18 * 0.18
            var total = subtotal;                // Total = subtotal

            var pedido = new PedidoWeb
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Hora = TimeOnly.FromDateTime(DateTime.Now),
                FechaIngreso = DateTime.Now,
                FechaModifica = DateTime.Now,
                UsuarioIngreso = userIdentifier,
                UsuarioModifico = userIdentifier,
                RucEmisor = "10413820532",
                IdLocal = "2",
                TipoPago = 2,
                EstadoPedido = 1,
                EstadoComprobante = 0,
                MedioPago = MediopagoCode,
                IdTransaccion = codigoTransaccion,
                SubtotalVenta = subtotal,
                Igv = igv,
                TotalVenta = total,
                TipoEntrega = 1,
                NroCaja = 0,
                TipoDoc = 1,
                IdCliente = usuario.IdCliente?.ToString() ?? "0",
                DireccionEntrega = Pedido.DireccionEntrega,
                FechaEntrega = Pedido.FechaEntrega,
                HoraEntrega = TimeOnly.Parse(Pedido.HoraEntrega),
                Indicaciones = Pedido.Indicaciones ?? ""
            };

            _context.PedidosWeb.Add(pedido);
            await _context.SaveChangesAsync();

            // ============================================================
            // 📌 PASO 4: DETALLES
            // ============================================================

            foreach (var temp in Temporales)
            {
                var codbarra = temp.Codbarra;
                if (string.IsNullOrWhiteSpace(codbarra))
                {
                    codbarra = temp.IdProducto?.ToString() ?? "0";
                }

                var detalle = new DetalleWeb
                {
                    IdPedidoWeb = pedido.IdPedidoWeb,
                    IdProducto = temp.IdProducto ?? 0,
                    Cantidad = temp.Cantidad ?? 0,
                    PrecioVenta = temp.PrecioVenta ?? 0,
                    ImporteTotal = (temp.PrecioVenta ?? 0) * (temp.Cantidad ?? 0),
                    Descripcion = temp.Nombre ?? "",
                    Codbarra = codbarra,
                    IdTipoAfectacion = temp.IdTipoAfectacion ?? "10",
                    IdTipoValorVenta = temp.IdTipoValorVenta ?? "01",
                    IdCodigoDetalle = temp.IdCodigoDetalle ?? "01",
                    IdTipoOperacion = null,
                    IdIsc = "0",
                    CodigoSunat = "999",
                    Igv = 0,
                    Isc = 0,
                    Subtotal = temp.SubtotalVenta ?? 0,
                    PrecioCompra = temp.PrecioCompra ?? 0,
                    FechaRegistro = DateTime.Now,
                    IdTienda = 2,
                    DirImagen = temp.Imagen ?? "",
                    SerieComprobante = "00",
                    NroComprobante = 0
                };

                _context.DetallesWeb.Add(detalle);
            }

            await _context.SaveChangesAsync();

            // ============================================================
            // 📌 PASO 5: DESCONTAR STOCK
            // ============================================================

            foreach (var temp in Temporales)
            {
                var productoTienda = await _context.ProductosTienda
                    .FirstOrDefaultAsync(pt => pt.IdProducto == temp.IdProducto && pt.IdTienda == 2);

                if (productoTienda != null)
                {
                    productoTienda.Stock = (productoTienda.Stock ?? 0) - (temp.Cantidad ?? 0);
                    if (productoTienda.Stock < 0) productoTienda.Stock = 0;
                    productoTienda.FechaModifica = DateOnly.FromDateTime(DateTime.Now);
                    productoTienda.HoraModifica = TimeOnly.FromDateTime(DateTime.Now);
                    _context.ProductosTienda.Update(productoTienda);
                }
            }

            await _context.SaveChangesAsync();

            // ============================================================
            // 📌 PASO 6: KARDEX
            // ============================================================

            foreach (var temp in Temporales)
            {
                var kardex = new Kardex
                {
                    IdProducto = temp.Codbarra ?? temp.IdProducto?.ToString() ?? "0",
                    ProductoId = temp.IdProducto ?? 0,
                    Cantidad = temp.Cantidad ?? 0,
                    CantidadSalida = temp.Cantidad ?? 0,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    IdAlmacen = 2,
                    IdLocal = 2,
                    Estado = 1,
                    RucEmisor = "10413820532",
                    IdUsuario = userIdentifier,
                    Observacion = $"Venta web - Pedido #{pedido.IdPedidoWeb}",
                    PrecioUnitario = temp.PrecioVenta ?? 0,
                    IdFlag = 2
                };

                _context.Kardex.Add(kardex);
            }

            await _context.SaveChangesAsync();

            // ============================================================
            // 📌 PASO 7: LIMPIAR CARRITO
            // ============================================================

            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier)
                .ToListAsync();

            if (temporales.Any())
            {
                _context.VentasTemporales.RemoveRange(temporales);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove("Carrito");

            // ============================================================
            // 📌 PASO 8: ENVIAR CORREOS (USANDO Items, QUE YA TIENE LOS DATOS)
            // ============================================================

            // ============================================================
            // 📌 PASO 8: ENVIAR CORREOS DE CONFIRMACIÓN
            // ============================================================

            try
            {
                var emailCliente = usuario.Email ?? "";
                if (string.IsNullOrWhiteSpace(emailCliente))
                {
                    throw new InvalidOperationException("El usuario no tiene un correo registrado para enviar la confirmación.");
                }

                // Correo para el cliente
                var subjectCliente = $"✅ Confirmación de pedido #{pedido.IdPedidoWeb} - Elfide.com";
                var bodyCliente = GenerarHtmlPedido(pedido, Items, usuario.Nombres ?? "Cliente");
                await _emailService.SendEmailAsync(emailCliente, subjectCliente, bodyCliente);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo al cliente: {ex.Message}");
            }

            try
            {
                var emailCliente = usuario.Email ?? "";

                // Correo para la empresa
                var subjectEmpresa = $"🛒 Nuevo pedido web #{pedido.IdPedidoWeb}";
                var bodyEmpresa = GenerarHtmlPedidoEmpresa(pedido, Items, emailCliente);
                await _emailService.SendEmailAsync("tiendaelfide@gmail.com", subjectEmpresa, bodyEmpresa);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo a la empresa: {ex.Message}");
            }

            // ============================================================
            // 📌 PASO 8.5: ENVIAR NOTIFICACIONES POR WHATSAPP
            // ============================================================

            try
            {
                // ✅ 1. Enviar WhatsApp al cliente (si tiene teléfono)
                if (!string.IsNullOrEmpty(usuario.Telefono))
                {
                    await _whatsAppService.EnviarPedidoAlClienteAsync(
                        usuario.Telefono,
                        pedido.IdPedidoWeb,
                        usuario.Nombres ?? "Cliente",
                        pedido.TotalVenta ?? 0,
                        pedido.DireccionEntrega ?? "",
                        pedido.FechaEntrega?.ToString("dd/MM/yyyy") ?? "",
                        pedido.HoraEntrega?.ToString("hh\\:mm") ?? ""
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando WhatsApp al cliente: {ex.Message}");
            }

            try
            {
                // ✅ 2. Enviar WhatsApp al administrador
                await _whatsAppService.EnviarPedidoAlAdminAsync(
                    pedido.IdPedidoWeb,
                    usuario.Email ?? "",
                    pedido.TotalVenta ?? 0,
                    pedido.IdTransaccion ?? "",
                    pedido.DireccionEntrega ?? "",
                    pedido.FechaEntrega?.ToString("dd/MM/yyyy") ?? "",
                    pedido.HoraEntrega?.ToString("hh\\:mm") ?? ""
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando WhatsApp al administrador: {ex.Message}");
            }

            // ============================================================
            // 📌 PASO 9: REDIRIGIR A ÉXITO
            // ============================================================

            TempData["Success"] = "✅ ¡Pago confirmado! Tu pedido está siendo procesado.";
            return RedirectToPage("/Pagos/Exitoso", new { id = pedido.IdPedidoWeb });

        }

        // ============================================================
        // 📌 MÉTODO AUXILIAR PARA RECARGAR DATOS
        // ============================================================

        private async Task CargarDatosPagina()
        {
            Horarios = await _context.HorariosEntrega
                .Where(h => h.Estado == 1)
                .OrderBy(h => h.HoraEntrega)
                .ToListAsync();

            var userIdentifier = GetUserIdentifier();
            Temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == userIdentifier && v.IdProducto.HasValue)
                .ToListAsync();

            Items = Temporales.Select(v => new CarritoItem
            {
                ProductoId = v.IdProducto ?? 0,
                Nombre = v.Nombre ?? "Producto",
                Imagen = v.Imagen ?? "",
                PrecioVenta = v.PrecioVenta ?? 0,
                Cantidad = (int)(v.Cantidad ?? 1),
                Codbarra = v.Codbarra ?? ""
            }).ToList();
        }

        // ============================================================
        // 📌 MÉTODOS PARA GENERAR HTML DE CORREOS
        // ============================================================

        private string GenerarHtmlPedido(PedidoWeb pedido, List<CarritoItem> items, string nombreCliente)
        {
            string baseUrl = "https://elfide.com/";

            var itemsHtml = string.Join("", items.Select(i => 
            {
                string imagenUrl = !string.IsNullOrEmpty(i.Imagen) 
                    ? $"{baseUrl}images/productos/{i.Imagen}" 
                    : $"{baseUrl}images/no-image.png";
                
                return $@"
                <tr>
                    <td style='padding: 8px; vertical-align: middle;'>
                        <img src='{imagenUrl}' alt='{i.Nombre}' style='width: 50px; height: 50px; object-fit: cover; border-radius: 4px; border: 1px solid #ddd;' />
                    </td>
                    <td style='padding: 8px; vertical-align: middle;'>{i.Nombre} x {i.Cantidad}</td>
                    <td style='padding: 8px; text-align: right; vertical-align: middle;'>S/ {i.Subtotal?.ToString("F2")}</td>
                </tr>";
            }));

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ background-color: #0B6299; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ padding: 20px; }}
                    .footer {{ margin-top: 20px; padding: 15px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
                    th {{ background-color: #0B6299; color: white; padding: 10px; text-align: left; }}
                    td {{ padding: 8px; border-bottom: 1px solid #eee; }}
                    .order-detail {{ background-color: #f9f9f9; padding: 10px; border-radius: 4px; margin: 10px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>✅ Pedido Confirmado</h2>
                    </div>
                    <div class='content'>
                        <p>Hola <strong>{nombreCliente}</strong>,</p>
                        <p>Tu pedido <strong>#{pedido.IdPedidoWeb}</strong> ha sido registrado exitosamente.</p>
                        <div class='order-detail'>
                            <p><strong>📅 Fecha:</strong> {pedido.Fecha:dd/MM/yyyy} - {pedido.Hora:HH:mm}</p>
                            <p><strong>💳 Método de pago:</strong> {MetodoDisplay}</p>
                            <p><strong>📍 Dirección de entrega:</strong> {pedido.DireccionEntrega}</p>
                            <p><strong>📦 Fecha de entrega:</strong> {pedido.FechaEntrega:dd/MM/yyyy} - {pedido.HoraEntrega:HH:mm}</p>
                            {(!string.IsNullOrEmpty(pedido.Indicaciones) ? $"<p><strong>📝 Indicaciones:</strong> {pedido.Indicaciones}</p>" : "")}
                            {(!string.IsNullOrEmpty(pedido.IdTransaccion) ? $"<p><strong>🔑 Código de transacción:</strong> {pedido.IdTransaccion}</p>" : "")}
                        </div>
                        <h3>🛒 Detalle del pedido</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th style='width: 60px;'>Imagen</th>
                                    <th>Producto</th>
                                    <th style='text-align: right;'>Subtotal</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan='2' style='text-align: right;'><strong style='color: #FFA600; font-size: 18px;'>Total</strong></td>
                                    <td style='text-align: right;'><strong style='color: #FFA600; font-size: 18px;'>S/ {pedido.TotalVenta?.ToString("F2")}</strong></td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='text-align: right; font-size: 12px; color: #888;'>IGV incluido en el total</td>
                                    <td style='text-align: right; font-size: 12px; color: #888;'></td>
                                </tr>
                            </tfoot>
                        </table>
                        <p style='margin-top: 20px;'>¡Gracias por tu compra! Pronto recibirás la confirmación de envío.</p>
                    </div>
                    <div class='footer'>
                        Elfide.com - Tu tienda online de confianza<br>
                        {DateTime.Now.Year} © Todos los derechos reservados.
                    </div>
                </div>
            </body>
            </html>
            ";
        }

        private string GenerarHtmlPedidoEmpresa(PedidoWeb pedido, List<CarritoItem> items, string emailCliente)
        {
            string baseUrl = "https://elfide.com/";

            var itemsHtml = string.Join("", items.Select(i => 
            {
                string imagenUrl = !string.IsNullOrEmpty(i.Imagen) 
                    ? $"{baseUrl}images/productos/{i.Imagen}" 
                    : $"{baseUrl}images/no-image.png";
                
                return $@"
                <tr>
                    <td style='padding: 8px; vertical-align: middle;'>
                        <img src='{imagenUrl}' alt='{i.Nombre}' style='width: 50px; height: 50px; object-fit: cover; border-radius: 4px; border: 1px solid #ddd;' />
                    </td>
                    <td style='padding: 8px; vertical-align: middle;'>{i.Nombre} x {i.Cantidad}</td>
                    <td style='padding: 8px; text-align: right; vertical-align: middle;'>S/ {i.Subtotal?.ToString("F2")}</td>
                </tr>";
            }));

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
                    .header {{ background-color: #FFA600; color: #070B12; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ padding: 20px; }}
                    .footer {{ margin-top: 20px; padding: 15px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
                    th {{ background-color: #0B6299; color: white; padding: 10px; text-align: left; }}
                    td {{ padding: 8px; border-bottom: 1px solid #eee; }}
                    .alert {{ background-color: #fff3cd; border-left: 4px solid #FFA600; padding: 12px; margin-bottom: 16px; border-radius: 4px; }}
                    .order-detail {{ background-color: #f9f9f9; padding: 10px; border-radius: 4px; margin: 10px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>🛒 NUEVO PEDIDO WEB</h2>
                        <p style='font-size:14px;'>Requiere atención del equipo de caja</p>
                    </div>
                    <div class='content'>
                        <div class='alert'>
                            <strong>⚠️ Pedido #{pedido.IdPedidoWeb}</strong> - {pedido.Fecha:dd/MM/yyyy} {pedido.Hora:HH:mm}
                        </div>
                        <div class='order-detail'>
                            <p><strong>👤 Cliente:</strong> {emailCliente}</p>
                            <p><strong>💳 Método de pago:</strong> {MetodoDisplay}</p>
                            <p><strong>🔑 Código de transacción:</strong> {pedido.IdTransaccion}</p>
                            <p><strong>📍 Dirección de entrega:</strong> {pedido.DireccionEntrega}</p>
                            <p><strong>📦 Fecha de entrega:</strong> {pedido.FechaEntrega:dd/MM/yyyy} - {pedido.HoraEntrega:HH:mm}</p>
                            {(!string.IsNullOrEmpty(pedido.Indicaciones) ? $"<p><strong>📝 Indicaciones:</strong> {pedido.Indicaciones}</p>" : "")}
                        </div>
                        <h3>🛒 Detalle del pedido</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th style='width: 60px;'>Imagen</th>
                                    <th>Producto</th>
                                    <th style='text-align: right;'>Subtotal</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan='2' style='text-align: right;'><strong style='color: #FFA600; font-size: 18px;'>Total</strong></td>
                                    <td style='text-align: right;'><strong style='color: #FFA600; font-size: 18px;'>S/ {pedido.TotalVenta?.ToString("F2")}</strong></td>
                                </tr>
                                <tr>
                                    <td colspan='2' style='text-align: right; font-size: 12px; color: #888;'>IGV incluido en el total</td>
                                    <td style='text-align: right; font-size: 12px; color: #888;'></td>
                                </tr>
                            </tfoot>
                        </table>
                        <p style='margin-top: 20px; color: #d9534f; font-weight: bold;'>Este pedido debe ser procesado en la caja.</p>
                    </div>
                    <div class='footer'>
                        Sistema de gestión de pedidos web - Elfide.com
                    </div>
                </div>
            </body>
            </html>
            ";
        }

        // ============================================================
        // 📌 CLASE AUXILIAR
        // ============================================================

        public class PedidoEnvio
        {
            public string DireccionEntrega { get; set; } = string.Empty;
            public DateOnly? FechaEntrega { get; set; }
            public string HoraEntrega { get; set; } = string.Empty;
            public string Indicaciones { get; set; } = string.Empty;
        }
    }
}