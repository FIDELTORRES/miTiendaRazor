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
    public class IndexModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public IndexModel(MiTiendaContext context)
        {
            _context = context;
        }

        public List<CarritoItem> Items { get; set; } = new();
        public decimal Subtotal => Items?.Sum(i => i.Subtotal) ?? 0;
        public decimal Igv => Subtotal * 0.18m;
        public decimal TotalConIgv => Subtotal + Igv;
        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();
        
        // ✅ Propiedad para saber si el usuario está autenticado
        public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        public async Task OnGetAsync()
        {
            await CargarCategorias();
            await CargarCarrito();
        }

        private async Task CargarCarrito()
        {
            string usuario = "web";

            var temporales = await _context.VentasTemporales
                .Where(v => v.UsuarioIngreso == usuario && v.IdProducto.HasValue)
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
            }

            // Si no hay items, redirigir al carrito
            if (!Items.Any())
            {
                Response.Redirect("/Web/Carrito");
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
    }
}