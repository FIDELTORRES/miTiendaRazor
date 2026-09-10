using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.ViewModels.Categoria;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Subcategoriapagina
{
    public class EditarModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditarModel(MiTiendaContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public SubcategoriapaginaViewModel SubcategoriaPagina { get; set; } = new();

        [BindProperty]
        public IFormFile? ImagenFile { get; set; }

        public List<miTienda.Models.Categoriapagina> CategoriasPagina { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entidad = await _context.SubcategoriasPagina
                .FirstOrDefaultAsync(c => c.IdSubcategoriapagina == id);

            if (entidad == null)
            {
                TempData["Error"] = "Subcategoría no encontrada";
                return RedirectToPage("./Index");
            }

            SubcategoriaPagina = new SubcategoriapaginaViewModel
            {
                IdSubcategoriapagina = entidad.IdSubcategoriapagina,
                Descripcion = entidad.Descripcion ?? string.Empty,
                IdCategoriapagina = entidad.IdCategoriapagina ?? 0,
                Imagen = entidad.Imagen,
                Estado = entidad.Estado ?? 1
            };

            await CargarListasAsync();
            return Page();
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
                // 🔥 CORREGIDO: VALIDACIÓN PARA EDICIÓN (EXCLUYE EL REGISTRO ACTUAL)
                var existe = await _context.SubcategoriasPagina
                    .AnyAsync(c => c.Descripcion == SubcategoriaPagina.Descripcion
                                   && c.IdCategoriapagina == SubcategoriaPagina.IdCategoriapagina
                                   && c.IdSubcategoriapagina != SubcategoriaPagina.IdSubcategoriapagina);  // ← EXCLUYE EL ACTUAL

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una subcategoría con esta descripción en la categoría página seleccionada.";
                    await CargarListasAsync();
                    return Page();
                }

                var entidad = await _context.SubcategoriasPagina
                    .FirstOrDefaultAsync(c => c.IdSubcategoriapagina == SubcategoriaPagina.IdSubcategoriapagina);

                if (entidad == null)
                {
                    TempData["Error"] = "Subcategoría no encontrada";
                    await CargarListasAsync();
                    return Page();
                }

                // Actualizar campos
                entidad.Descripcion = SubcategoriaPagina.Descripcion;
                entidad.IdCategoriapagina = SubcategoriaPagina.IdCategoriapagina;
                entidad.Estado = SubcategoriaPagina.Estado;

                // Procesar nueva imagen
                if (ImagenFile != null && ImagenFile.Length > 0)
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(entidad.Imagen))
                    {
                        string oldPath = Path.Combine(_environment.WebRootPath, "images", "subcategorias", entidad.Imagen);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Guardar nueva imagen
                    string extension = Path.GetExtension(ImagenFile.FileName).ToLowerInvariant();
                    string nombreImagen = $"{Guid.NewGuid():N}{extension}";
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "subcategorias");
                    Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, nombreImagen);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagenFile.CopyToAsync(stream);
                    }

                    entidad.Imagen = nombreImagen;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Subcategoría '{entidad.Descripcion}' actualizada exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error al actualizar: {ex.Message}";
                await CargarListasAsync();
                return Page();
            }
        }

        private async Task CargarListasAsync()
        {
            CategoriasPagina = await _context.CategoriasPagina
                .Where(c => c.Estado == 1)
                .OrderBy(c => c.Descripcion)
                .ToListAsync();
        }
    }
}