using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.ViewModels.Categoria;
using System.IO;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Categoriapropia
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
        public CategoriapropiaViewModel CategoriaPropia { get; set; } = new();

        [BindProperty]
        public IFormFile? ImagenFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categoria = await _context.CategoriasPropias
                .FirstOrDefaultAsync(c => c.IdCategoriapropia == id);

            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada";
                return RedirectToPage("./Index");
            }

            CategoriaPropia = new CategoriapropiaViewModel
            {
                IdCategoriapropia = categoria.IdCategoriapropia,
                Descripcion = categoria.Descripcion,
                Imagen = categoria.Imagen,
                Estado = categoria.Estado
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 🔥 CORREGIDO: Validación para EDICIÓN (excluye el registro actual)
                // Usar EF.Functions.Like para comparación case-insensitive
                var existe = await _context.CategoriasPropias
                    .AnyAsync(c => EF.Functions.Like(c.Descripcion, CategoriaPropia.Descripcion)
                                   && c.IdCategoriapropia != CategoriaPropia.IdCategoriapropia);

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una categoría con esta descripción.";
                    return Page();
                }

                var categoria = await _context.CategoriasPropias
                    .FirstOrDefaultAsync(c => c.IdCategoriapropia == CategoriaPropia.IdCategoriapropia);

                if (categoria == null)
                {
                    TempData["Error"] = "Categoría no encontrada";
                    return Page();
                }

                // Actualizar campos
                categoria.Descripcion = CategoriaPropia.Descripcion;
                categoria.Estado = CategoriaPropia.Estado;

                // Procesar nueva imagen
                if (ImagenFile != null && ImagenFile.Length > 0)
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(categoria.Imagen))
                    {
                        string oldPath = Path.Combine(_environment.WebRootPath, "images", "categorias", categoria.Imagen);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Guardar nueva imagen
                    string extension = Path.GetExtension(ImagenFile.FileName).ToLowerInvariant();
                    string nombreImagen = $"{Guid.NewGuid():N}{extension}";
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "categorias");
                    Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, nombreImagen);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagenFile.CopyToAsync(stream);
                    }

                    categoria.Imagen = nombreImagen;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Categoría '{categoria.Descripcion}' actualizada exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error al actualizar: {ex.Message}";
                return Page();
            }
        }
    }
}