using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using miTienda.ViewModels.Categoria;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CategoriapropiaEntity = miTienda.Models.Categoriapropia;
using CategoriapaginaEntity = miTienda.Models.Categoriapagina;

namespace miTienda.Pages.Admin.Categorias.Categoriapagina
{
    public class EditarModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public EditarModel(MiTiendaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CategoriapaginaViewModel CategoriaPagina { get; set; } = new();

        // ============================================================
        // 📌 LISTAS PARA SELECTS
        // ============================================================

        // Categorías Propias para el select
        public List<CategoriapropiaEntity> CategoriasPropias { get; set; } = new();

        // 🔥 Subcategorías para mostrar el valor guardado en el select
        public List<Subcategoria> Subcategorias { get; set; } = new();

        // ============================================================
        // 📌 MÉTODO GET: Carga los datos del registro a editar
        // ============================================================
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Buscar el registro en la base de datos
            var item = await _context.CategoriasPagina
                .FirstOrDefaultAsync(cp => cp.IdCategoriapagina == id);

            if (item == null)
            {
                TempData["Error"] = "Registro no encontrado";
                return RedirectToPage("./Index");
            }

            // 2. Mapear al ViewModel
            CategoriaPagina = new CategoriapaginaViewModel
            {
                IdCategoriapagina = item.IdCategoriapagina,
                IdCategoriapropia = item.IdCategoriapropia,
                IdSubcategoria = item.IdSubcategoria,
                Descripcion = item.Descripcion,
                Imagen = item.Imagen,
                Estado = item.Estado
            };

            // 3. Cargar las listas para los selects
            await CargarListasAsync();

            // 🔥 4. Cargar subcategorías para mostrar el valor guardado
            Subcategorias = await _context.Subcategorias
                .Where(s => s.Estado == 1)
                .OrderBy(s => s.Descripcion)
                .ToListAsync();

            return Page();
        }

        // ============================================================
        // 📌 MÉTODO POST: Actualiza el registro
        // ============================================================
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return Page();
            }

            try
            {
                // Verificar duplicados
                var existe = await _context.CategoriasPagina
                    .AnyAsync(c => c.IdCategoriapropia == CategoriaPagina.IdCategoriapropia
                                   && c.IdSubcategoria == CategoriaPagina.IdSubcategoria
                                   && c.IdCategoriapagina != CategoriaPagina.IdCategoriapagina);

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una categoría página con esta combinación.";
                    await CargarListasAsync();
                    return Page();
                }

                // Obtener el registro existente
                var item = await _context.CategoriasPagina
                    .FirstOrDefaultAsync(cp => cp.IdCategoriapagina == CategoriaPagina.IdCategoriapagina);

                if (item == null)
                {
                    TempData["Error"] = "Registro no encontrado";
                    await CargarListasAsync();
                    return Page();
                }

                // Actualizar campos
                item.IdCategoriapropia = CategoriaPagina.IdCategoriapropia;
                item.IdSubcategoria = CategoriaPagina.IdSubcategoria;
                item.Descripcion = CategoriaPagina.Descripcion;
                item.Imagen = CategoriaPagina.Imagen;
                item.Estado = CategoriaPagina.Estado;

                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Categoría Página actualizada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error al actualizar: {ex.Message}";
                await CargarListasAsync();
                return Page();
            }
        }

        // ============================================================
        // 📌 MÉTODOS AUXILIARES
        // ============================================================

        private async Task CargarListasAsync()
        {
            CategoriasPropias = await _context.CategoriasPropias
                .Where(c => c.Estado == 1)
                .OrderBy(c => c.Descripcion)
                .ToListAsync();
        }

        // ============================================================
        // 📌 HANDLER AJAX: Buscar subcategorías (SIN FILTRO)
        // ============================================================
        public async Task<IActionResult> OnGetBuscarSubcategoriasAsync(string? term)
        {
            var query = _context.Subcategorias.Where(s => s.Estado == 1).AsQueryable();

            // 🔥 BUSCAR POR TÉRMINO (código o descripción)
            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(s => s.IdSubcategoria.ToString().Contains(term)
                                         || (s.Descripcion != null && s.Descripcion.Contains(term)));
            }

            // Devolver 20 resultados máximo
            var subcategorias = await query
                .OrderBy(s => s.Descripcion)
                .Take(20)
                .Select(s => new { id = s.IdSubcategoria.ToString(), text = $"{s.IdSubcategoria} - {s.Descripcion}" })
                .ToListAsync();

            return new JsonResult(new { results = subcategorias });
        }
    }
}