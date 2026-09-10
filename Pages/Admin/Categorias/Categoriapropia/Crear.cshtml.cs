using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;  
using miTienda.Data;
using miTienda.ViewModels.Categoria;
using System.IO;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Categorias.Categoriapropia
{
    public class CrearModel : PageModel
    {
        private readonly MiTiendaContext _context;
        private readonly IWebHostEnvironment _environment;

        public CrearModel(MiTiendaContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public CategoriapropiaViewModel CategoriaPropia { get; set; } = new();

        [BindProperty]
        public IFormFile? ImagenFile { get; set; }

        public void OnGet()
        {
            // Inicializar estado por defecto
            CategoriaPropia.Estado = 1;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
               // Verificar duplicados
               var existe = await _context.CategoriasPropias
                  .AnyAsync(c => c.Descripcion.ToLower() == CategoriaPropia.Descripcion.ToLower());

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una categoría con esta descripción.";
                    return Page();
                }

                // Crear entidad
                var categoria = new Models.Categoriapropia
                {
                    Descripcion = CategoriaPropia.Descripcion,
                    Estado = CategoriaPropia.Estado
                };

                // Procesar imagen
                if (ImagenFile != null && ImagenFile.Length > 0)
                {
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

                _context.CategoriasPropias.Add(categoria);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Categoría '{categoria.Descripcion}' creada exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error al guardar: {ex.Message}";
                return Page();
            }
        }
    }
}