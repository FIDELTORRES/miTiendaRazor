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
        public SubcategoriapaginaViewModel SubcategoriaPagina { get; set; } = new();

        [BindProperty]
        public IFormFile? ImagenFile { get; set; }

        public List<Models.Categoriapagina> CategoriasPagina { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarListasAsync();
            SubcategoriaPagina.Estado = 1;
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
                var existe = await _context.SubcategoriasPagina
                    .AnyAsync(c => c.Descripcion == SubcategoriaPagina.Descripcion
                                   && c.IdCategoriapagina == SubcategoriaPagina.IdCategoriapagina);

                if (existe)
                {
                    TempData["Error"] = "⚠️ Ya existe una subcategoría con esta descripción en la categoría página seleccionada.";
                    await CargarListasAsync();
                    return Page();
                }

                // Crear entidad
                var entidad = new Models.Subcategoriapagina
                {
                    Descripcion = SubcategoriaPagina.Descripcion,
                    IdCategoriapagina = SubcategoriaPagina.IdCategoriapagina,
                    Estado = SubcategoriaPagina.Estado
                };

                // Procesar imagen
                if (ImagenFile != null && ImagenFile.Length > 0)
                {
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

                _context.SubcategoriasPagina.Add(entidad);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Subcategoría '{entidad.Descripcion}' creada exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error al guardar: {ex.Message}";
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