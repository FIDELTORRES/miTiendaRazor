using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages;

public class ProductosModel : PageModel
{
    private readonly MiTiendaContext _context;

    public ProductosModel(MiTiendaContext context)
    {
        _context = context;
    }

    // ============================================================
    // 📌 PROPIEDADES PARA LA VISTA
    // ============================================================
    public List<ProductoTienda> ProductosTienda { get; set; } = new();

    // 📌 Propiedades para paginación
    public int PaginaActual { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public int RegistrosPorPagina { get; set; } = 10;

    // ============================================================
    // 📌 PROPIEDADES PARA FILTROS (se mantienen al navegar)
    // ============================================================
    [BindProperty(SupportsGet = true)]
    public int? TiendaFiltro { get; set; } = 2;  // Por defecto tienda 2

    [BindProperty(SupportsGet = true)]
    public string? CodbarraFiltro { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? NombreFiltro { get; set; }

    // ============================================================
    // 📌 MÉTODO PRINCIPAL (se ejecuta al cargar la página)
    // ============================================================
    public async Task OnGetAsync(int? pagina)
    {
        // 📌 Establecer página actual
        PaginaActual = pagina ?? 1;
        if (PaginaActual < 1) PaginaActual = 1;

        // ============================================================
        // 📌 1. CONSTRUIR LA CONSULTA CON FILTROS
        // ============================================================
        var query = _context.ProductosTienda
            .Include(p => p.Producto)  // Incluir datos del producto
            .AsQueryable();            // Convertir a IQueryable para filtrar

        // 📌 Filtro por tienda (si se seleccionó)
        if (TiendaFiltro.HasValue && TiendaFiltro.Value > 0)
        {
            query = query.Where(p => p.IdTienda == TiendaFiltro.Value);
        }

        // 📌 Filtro por código de barras (búsqueda parcial)
        if (!string.IsNullOrEmpty(CodbarraFiltro))
        {
            query = query.Where(p => p.Codbarra != null && 
                                     p.Codbarra.Contains(CodbarraFiltro));
        }
        // 📌 Filtro por nombre de producto (búsqueda parcial)
        if (!string.IsNullOrEmpty(NombreFiltro))
        {
            query = query.Where(p => p.Producto != null && 
                                     p.Producto.Nombre.Contains(NombreFiltro));
        }

        // ============================================================
        // 📌 2. CONTAR TOTAL DE REGISTROS (para la paginación)
        // ============================================================
        TotalRegistros = await query.CountAsync();
        TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

        // 📌 Asegurar que la página actual no exceda el total
        if (PaginaActual > TotalPaginas && TotalPaginas > 0)
        {
            PaginaActual = TotalPaginas;
        }

        // ============================================================
        // 📌 3. OBTENER LOS REGISTROS DE LA PÁGINA ACTUAL
        // ============================================================
        ProductosTienda = await query
            .OrderBy(p => p.Id)                           // Ordenar por ID
            .Skip((PaginaActual - 1) * RegistrosPorPagina) // Saltar registros
            .Take(RegistrosPorPagina)                      // Tomar 10 registros
            .ToListAsync();
    }
    // ============================================================
    // 📌 MÉTODO PARA LIMPIAR FILTROS
    // ============================================================
    public IActionResult OnGetLimpiarFiltros()
    {
        return RedirectToPage(new { 
            tiendaFiltro = 2, 
            codbarraFiltro = "", 
            nombreFiltro = "",
            pagina = 1 
        });
    }

    // ============================================================
    // 📌 MÉTODO PARA CAMBIAR ESTADO (Eliminación Lógica)
    // ============================================================
    public async Task<IActionResult> OnGetToggleEstadoAsync(int id, int estado)
    {
        try
        {
            // Buscar el producto en la tienda
            var productoTienda = await _context.ProductosTienda
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productoTienda == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToPage();
            }

            // Cambiar el estado
            productoTienda.Estado = estado;
            productoTienda.FechaModifica = DateOnly.FromDateTime(DateTime.Now);
            productoTienda.HoraModifica = TimeOnly.FromDateTime(DateTime.Now);

            await _context.SaveChangesAsync();

            string mensaje = estado == 1 ? "activado" : "desactivado";
            TempData["Success"] = $"Producto {mensaje} correctamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cambiar estado: {ex.Message}";
        }

        // Mantener los filtros después de la operación
        return RedirectToPage(new
        {
            tiendaFiltro = TiendaFiltro,
            codbarraFiltro = CodbarraFiltro,
            nombreFiltro = NombreFiltro,
            pagina = PaginaActual
        });
    }
}