using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.ViewModels.Categoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Subcategoriapagina;

public class IndexModel : PageModel
{
    private readonly MiTiendaContext _context;

    public IndexModel(MiTiendaContext context)
    {
        _context = context;
    }

    public List<SubcategoriapaginaViewModel> Subcategorias { get; set; } = new();
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int RegistrosPorPagina { get; set; } = 10;

    [BindProperty(SupportsGet = true)]
    public string? DescripcionFiltro { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EstadoFiltro { get; set; }

    public async Task OnGetAsync(int? pagina)
    {
        PaginaActual = pagina ?? 1;
        if (PaginaActual < 1) PaginaActual = 1;

        // 🔥 CAMBIO 1: Incluir la relación con Categoriapagina
        var query = _context.SubcategoriasPagina
            .Include(s => s.Categoriapagina)
            .AsQueryable();

        // 🔥 CAMBIO 2: Búsqueda inteligente en múltiples campos
        if (!string.IsNullOrEmpty(DescripcionFiltro))
        {
            var termino = DescripcionFiltro.ToLower();
            query = query.Where(c =>
                c.IdSubcategoriapagina.ToString().Contains(termino) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(termino)) ||
                (c.Categoriapagina != null && c.Categoriapagina.Descripcion.ToLower().Contains(termino))
            );
        }

        if (EstadoFiltro.HasValue)
        {
            query = query.Where(c => c.Estado == EstadoFiltro.Value);
        }

        query = query.OrderBy(c => c.Descripcion);

        TotalRegistros = await query.CountAsync();
        TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

        if (PaginaActual > TotalPaginas && TotalPaginas > 0)
        {
            PaginaActual = TotalPaginas;
        }

        var subcategorias = await query
            .Skip((PaginaActual - 1) * RegistrosPorPagina)
            .Take(RegistrosPorPagina)
            .ToListAsync();

        // 🔥 CAMBIO 3: Usar la relación directamente
        Subcategorias = subcategorias.Select(s => new SubcategoriapaginaViewModel
        {
            IdSubcategoriapagina = s.IdSubcategoriapagina,
            Descripcion = s.Descripcion ?? string.Empty,
            IdCategoriapagina = s.IdCategoriapagina ?? 0,
            Estado = s.Estado ?? 1,
            CategoriaPaginaDesc = s.Categoriapagina?.Descripcion ?? "Sin categoría"
        }).ToList();
    }

    // ============================================================
    // 📌 HANDLER PARA CAMBIAR ESTADO
    // ============================================================
    public async Task<IActionResult> OnGetToggleEstadoAsync(int id, int estado)
    {
        try
        {
            var entidad = await _context.SubcategoriasPagina
                .FirstOrDefaultAsync(c => c.IdSubcategoriapagina == id);

            if (entidad == null)
            {
                TempData["Error"] = "Subcategoría no encontrada";
                return RedirectToPage(new
                {
                    pagina = PaginaActual,
                    descripcionFiltro = DescripcionFiltro,
                    estadoFiltro = EstadoFiltro
                });
            }

            entidad.Estado = estado;
            await _context.SaveChangesAsync();

            string mensaje = estado == 1 ? "activada" : "desactivada";
            TempData["Success"] = $"Subcategoría '{entidad.Descripcion}' {mensaje} correctamente";
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