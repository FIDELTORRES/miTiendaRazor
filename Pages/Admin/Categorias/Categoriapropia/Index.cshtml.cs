using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.ViewModels.Categoria;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Categoriapropia
{
    public class IndexModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public IndexModel(MiTiendaContext context)
        {
            _context = context;
        }

        public List<CategoriapropiaViewModel> Categorias { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int RegistrosPorPagina { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? DescripcionFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EstadoFiltro { get; set; }

        public async Task OnGetAsync(int? pagina)
        {
            PaginaActual = pagina ?? 1;
            if (PaginaActual < 1) PaginaActual = 1;

            var query = _context.CategoriasPropias.AsQueryable();

            if (!string.IsNullOrEmpty(DescripcionFiltro))
            {
                query = query.Where(c => c.Descripcion.Contains(DescripcionFiltro));
            }

            if (EstadoFiltro.HasValue)
            {
                query = query.Where(c => c.Estado == EstadoFiltro.Value);
                
            }

            query = query.OrderBy(c => c.Descripcion);

            int totalRegistros = await query.CountAsync();
            TotalPaginas = (int)Math.Ceiling((double)totalRegistros / RegistrosPorPagina);

            if (PaginaActual > TotalPaginas && TotalPaginas > 0)
            {
                PaginaActual = TotalPaginas;
            }

            var categorias = await query
                .Skip((PaginaActual - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            // ✅ CORREGIDO: c.Estado es int, no necesita ??
            Categorias = categorias.Select(c => new CategoriapropiaViewModel
            {
                IdCategoriapropia = c.IdCategoriapropia,
                Descripcion = c.Descripcion,
                Estado = c.Estado  // ← Sin operador ??
            }).ToList();
        }

        public async Task<IActionResult> OnGetToggleEstadoAsync(int id, int estado)
        {
            try
            {
                var categoria = await _context.CategoriasPropias
                    .FirstOrDefaultAsync(c => c.IdCategoriapropia == id);

                if (categoria == null)
                {
                    TempData["Error"] = "Categoría no encontrada";
                    return RedirectToPage();
                }

                categoria.Estado = estado;
                await _context.SaveChangesAsync();

                string mensaje = estado == 1 ? "activada" : "desactivada";
                TempData["Success"] = $"Categoría '{categoria.Descripcion}' {mensaje} correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cambiar estado: {ex.Message}";
            }

            return RedirectToPage(new
            {
                pagina = PaginaActual,
                descripcionFiltro = DescripcionFiltro,
                estadoFiltro = EstadoFiltro
            });
        }

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