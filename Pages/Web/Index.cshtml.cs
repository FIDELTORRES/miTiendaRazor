using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Web  // ✅ CAMBIAR DE Pages.Pagos a Pages.Web
{
    public class IndexModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public IndexModel(MiTiendaContext context)
        {
            _context = context;
        }

        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();
        public PagedResult<ProductoTienda> ProductosPaginados { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Busqueda { get; set; } = string.Empty;

        public async Task OnGetAsync(int? categoriaId, string busqueda)
        {
            CategoriaId = categoriaId;
            Busqueda = busqueda?.Trim() ?? string.Empty;

            // 1. Cargar categorías
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

            // 2. Consulta de productos
            const int pageSize = 12;

            var query = _context.ProductosTienda
                .Include(pt => pt.Producto)
                .Where(pt => pt.Estado == 1 && pt.IdTienda == 2);

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(pt => pt.SubcategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                bool esNumero = int.TryParse(Busqueda, out int idBuscado);
                
                query = query.Where(pt => 
                    (pt.Producto != null && pt.Producto.Nombre != null && 
                     pt.Producto.Nombre.ToLower().Contains(Busqueda.ToLower())) ||
                    (pt.Producto != null && pt.Producto.Descripcion != null && 
                     pt.Producto.Descripcion.ToLower().Contains(Busqueda.ToLower())) ||
                    (pt.Codbarra != null && pt.Codbarra.ToLower().Contains(Busqueda.ToLower())) ||
                    (esNumero && pt.IdProducto == idBuscado)
                );
            }

            query = query.OrderBy(pt => pt.Producto!.Nombre);

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ProductosPaginados = new PagedResult<ProductoTienda>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = PageNumber,
                PageSize = pageSize
            };

            ViewData["CategoriasNivel1"] = CategoriasNivel1;
            ViewData["Busqueda"] = Busqueda;
        }
    }
}