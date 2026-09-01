using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using miTienda.ViewModels.Categoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CategoriapropiaEntity = miTienda.Models.Categoriapropia;
using CategoriapaginaEntity = miTienda.Models.Categoriapagina;

namespace miTienda.Pages.Admin.Categorias.Categoriapagina
{
    public class CrearModel : PageModel
    {
        private readonly MiTiendaContext _context;

        public CrearModel(MiTiendaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CategoriapaginaViewModel CategoriaPagina { get; set; } = new();

        public List<CategoriapropiaEntity> CategoriasPropias { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarListasAsync();
            CategoriaPagina.Estado = 1;
        }

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
                                   && c.IdSubcategoria == CategoriaPagina.IdSubcategoria);

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una categoría página con esta combinación de Categoría Propia y Subcategoría.";
                    await CargarListasAsync();
                    return Page();
                }

                var categoriaPagina = new CategoriapaginaEntity
                {
                    IdCategoriapropia = CategoriaPagina.IdCategoriapropia,
                    IdSubcategoria = CategoriaPagina.IdSubcategoria,
                    Descripcion = CategoriaPagina.Descripcion,
                    Imagen = CategoriaPagina.Imagen,
                    Estado = CategoriaPagina.Estado
                };

                _context.CategoriasPagina.Add(categoriaPagina);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Categoría Página creada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error al guardar: {ex.Message}";
                await CargarListasAsync();
                return Page();
            }
        }

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

            var subcategorias = await query
                .OrderBy(s => s.Descripcion)
                .Take(20)
                .Select(s => new { id = s.IdSubcategoria.ToString(), text = $"{s.IdSubcategoria} - {s.Descripcion}" })
                .ToListAsync();

            return new JsonResult(new { results = subcategorias });
        }
    }
}