using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Web
{
    /// <summary>
    /// Página de detalle de un producto específico.
    /// </summary>
    public class DetalleModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public DetalleModel(MiTiendaContext context)
        {
            _context = context;
        }

        // Producto mostrado en la vista (incluye datos de producto principal)
        public ProductoTienda? Producto { get; set; }

        // ✅ Agregar propiedad para las categorías
        public List<CategoriaNivel1> CategoriasNivel1 { get; set; } = new();

        /// <summary>
        /// Carga el producto con el ID proporcionado.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Cargar el producto
            Producto = await _context.ProductosTienda
                .Include(pt => pt.Producto)
                .FirstOrDefaultAsync(pt => pt.IdProducto == id && pt.IdTienda == 2);

            if (Producto == null)
            {
                return NotFound();
            }

            // 2. Cargar categorías para el menú
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

            // 3. Pasar datos al layout mediante ViewData
            ViewData["CategoriasNivel1"] = CategoriasNivel1;

            return Page();
        }
    }
}