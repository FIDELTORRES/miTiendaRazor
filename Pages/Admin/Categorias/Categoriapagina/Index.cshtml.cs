using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Categoriapagina
{
    public class IndexModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public IndexModel(MiTiendaContext context)
        {
            _context = context;
        }

        public List<miTienda.Models.Categoriapagina> CategoriasPagina { get; set; } = new();
        public List<Subcategoria> Subcategorias { get; set; } = new();

        public int PaginaActual { get; set; } = 1;
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int RegistrosPorPagina { get; set; } = 10;

        // 🔥 NUEVAS PROPIEDADES PARA FILTROS
        [BindProperty(SupportsGet = true)]
        public string? DescripcionFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EstadoFiltro { get; set; }

        public async Task OnGetAsync(int? pagina)
        {
            PaginaActual = pagina ?? 1;
            if (PaginaActual < 1) PaginaActual = 1;

            // Cargar subcategorías para mostrar sus descripciones
            Subcategorias = await _context.Subcategorias
                .Where(s => s.Estado == 1)
                .OrderBy(s => s.Descripcion)
                .ToListAsync();

            // 🔥 CONSULTA PRINCIPAL CON INCLUDE
            var query = _context.CategoriasPagina
                .Include(c => c.Categoriapropia)
                .AsQueryable();

            // 🔥 FILTRO INTELIGENTE
            if (!string.IsNullOrEmpty(DescripcionFiltro))
            {
                var termino = DescripcionFiltro.ToLower();
                query = query.Where(c =>
                    c.IdCategoriapagina.ToString().Contains(termino) ||
                    (c.Descripcion != null && c.Descripcion.ToLower().Contains(termino)) ||
                    (c.Categoriapropia != null && c.Categoriapropia.Descripcion.ToLower().Contains(termino)) ||
                    (c.IdSubcategoria != null && c.IdSubcategoria.ToLower().Contains(termino))
                );
            }

            // 🔥 FILTRO POR ESTADO
            if (EstadoFiltro.HasValue)
            {
                query = query.Where(c => c.Estado == EstadoFiltro.Value);
            }

            // 🔥 ORDENAMIENTO
            query = query.OrderBy(c => c.Descripcion);

            TotalRegistros = await query.CountAsync();
            TotalPaginas = (int)System.Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

            if (PaginaActual > TotalPaginas && TotalPaginas > 0)
            {
                PaginaActual = TotalPaginas;
            }

            CategoriasPagina = await query
                .Skip((PaginaActual - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();
        }

        // ============================================================
        // 📌 HANDLER PARA LIMPIAR FILTROS
        // ============================================================
        public IActionResult OnGetLimpiarFiltros()
        {
            return RedirectToPage(new
            {
                pagina = 1,
                descripcionFiltro = "",
                estadoFiltro = (int?)null
            });
        }
    }
}