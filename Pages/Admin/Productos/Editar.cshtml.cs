using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace miTienda.Pages.Admin.Productos;

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
    public ProductoEditViewModel Producto { get; set; } = new();

    // ============================================================
    // 📌 LISTAS PARA SELECTS
    // ============================================================
    public List<Subcategoria> Subcategorias { get; set; } = new();
    public List<Categoriapropia> CategoriasPropias { get; set; } = new();
    public List<Categoriapagina> CategoriasPagina { get; set; } = new();
    public List<Subcategoriapagina> SubcategoriasPagina { get; set; } = new();
    public List<Marca> Marcas { get; set; } = new();
    public List<Medida> Medidas { get; set; } = new();

    // ============================================================
    // 📌 MÉTODO GET - CARGAR DATOS DEL PRODUCTO
    // ============================================================
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
        {
            TempData["Error"] = "Producto no encontrado";
            await CargarListasAsync();
            return Page();
        }

        var productosTienda = await _context.ProductosTienda
            .Where(pt => pt.IdProducto == id)
            .ToListAsync();

        Producto = new ProductoEditViewModel
        {
            IdProducto = producto.IdProducto,
            Codigo = producto.Codigo,
            Codbarra = producto.Codbarra,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Imagen = producto.Imagen,
            IdSubcategoria = producto.IdCategoria,
            IdSubcategoriaPagina = producto.IdSubcategoria ?? 0,
            IdMedidas = producto.IdMedidas,
            StockMinimo = producto.StockMinimo,
            StockMaximo = producto.StockMaximo,
            MargenPorcentaje = producto.MargenPorcentaje,
            FechaIngreso = producto.FechaIngreso,
            Estado = producto.Estado ?? 1,
            Categorias = producto.Categorias,               // Nivel 1
            CategoriaPaginaId = producto.CategoriaPaginaId, // Nivel 2
            IdMarca = producto.IdMarca,
            IdTipoAfectacion = producto.IdTipoAfectacion ?? "10",
            IdTipoFactura = producto.IdTipoFactura ?? "0101",
            IdCodigoDetalle = producto.IdCodigoDetalle ?? "01",
            IdTipoTributo = producto.IdTipoTributo ?? "1000",
            IdTipoValorVenta = producto.IdTipoValorVenta ?? "01",
            Impuesto = producto.Impuesto ?? 18.00m,
            ProductosTienda = productosTienda.Select(pt => new ProductoTiendaEdit
            {
                Id = pt.Id,
                IdTienda = pt.IdTienda,
                NombreTienda = pt.IdTienda == 2 ? "Tienda Principal" :
                              pt.IdTienda == 3 ? "Almacén" : $"Tienda {pt.IdTienda}",
                Codbarra = pt.Codbarra,
                Stock = pt.Stock,
                PrecioVenta = pt.PrecioVenta,
                PrecioCompra = pt.PrecioCompra,
                Estado = pt.Estado ?? 1
            }).ToList()
        };

        await CargarListasAsync();
        return Page();
    }

    // ============================================================
    // 📌 MÉTODO POST - ACTUALIZAR PRODUCTO
    // ============================================================
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return Page();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            string nombreImagen = Producto.Imagen ?? string.Empty;

            // Procesar nueva imagen
            if (Producto.ImagenFile != null && Producto.ImagenFile.Length > 0)
            {
                if (Producto.ImagenFile.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "La imagen no puede superar los 2MB";
                    await CargarListasAsync();
                    return Page();
                }

                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(Producto.ImagenFile.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    TempData["Error"] = "Formato de imagen no permitido. Use: JPG, PNG, WEBP";
                    await CargarListasAsync();
                    return Page();
                }

                if (!string.IsNullOrEmpty(Producto.Imagen))
                {
                    string oldPath = Path.Combine(_environment.WebRootPath, "images", "productos", Producto.Imagen);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                nombreImagen = $"{Guid.NewGuid():N}{extension}";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "productos");
                Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, nombreImagen);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Producto.ImagenFile.CopyToAsync(fileStream);
                }
            }

            // Actualizar producto principal
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == Producto.IdProducto);

            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado";
                await CargarListasAsync();
                return Page();
            }

            producto.Codigo = Producto.Codigo;
            producto.Codbarra = Producto.Codbarra;
            producto.Nombre = Producto.Nombre;
            producto.Descripcion = Producto.Descripcion;
            producto.Imagen = nombreImagen;
            producto.IdCategoria = Producto.IdSubcategoria;
            producto.IdSubcategoria = Producto.IdSubcategoriaPagina; // Nivel 3
            producto.CategoriaPaginaId = Producto.CategoriaPaginaId; // Nivel 2
            producto.Categorias = Producto.Categorias;               // Nivel 1
            producto.IdMedidas = Producto.IdMedidas;
            producto.IdMarca = Producto.IdMarca;
            producto.StockMinimo = Producto.StockMinimo;
            producto.StockMaximo = Producto.StockMaximo;
            producto.MargenPorcentaje = Producto.MargenPorcentaje ?? 0;
            producto.IdTipoAfectacion = Producto.IdTipoAfectacion ?? "10";
            producto.IdTipoFactura = Producto.IdTipoFactura ?? "0101";
            producto.IdCodigoDetalle = Producto.IdCodigoDetalle ?? "01";
            producto.IdTipoTributo = Producto.IdTipoTributo ?? "1000";
            producto.IdTipoValorVenta = Producto.IdTipoValorVenta ?? "01";
            producto.Impuesto = Producto.Impuesto ?? 18.00m;
            producto.Estado = Producto.Estado ?? 1;

            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();

            // Actualizar productotienda
            foreach (var ptEdit in Producto.ProductosTienda)
            {
                var productoTienda = await _context.ProductosTienda
                    .FirstOrDefaultAsync(pt => pt.Id == ptEdit.Id);

                if (productoTienda != null)
                {
                    if (ptEdit.Stock.HasValue)
                        productoTienda.Stock = ptEdit.Stock.Value;

                    if (ptEdit.PrecioVenta.HasValue)
                        productoTienda.PrecioVenta = ptEdit.PrecioVenta.Value;

                    if (ptEdit.PrecioCompra.HasValue)
                        productoTienda.PrecioCompra = ptEdit.PrecioCompra.Value;

                    productoTienda.Codbarra = ptEdit.Codbarra ?? Producto.Codbarra;
                    productoTienda.Estado = ptEdit.Estado ?? 1;
                    productoTienda.FechaModifica = DateOnly.FromDateTime(DateTime.Now);
                    productoTienda.HoraModifica = TimeOnly.FromDateTime(DateTime.Now);

                    _context.ProductosTienda.Update(productoTienda);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"✅ Producto '{producto.Nombre}' actualizado exitosamente";
            return RedirectToPage("/Admin/Productos/Productos");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"❌ Error al actualizar el producto: {ex.Message}";
            await CargarListasAsync();
            return Page();
        }
    }

    // ============================================================
    // 📌 MÉTODOS AUXILIARES
    // ============================================================
    private async Task CargarListasAsync()
    {
        Subcategorias = await _context.Subcategorias
            .Where(s => s.Estado == 1)
            .OrderBy(s => s.Descripcion)
            .ToListAsync();

        CategoriasPropias = await _context.CategoriasPropias
            .Where(c => c.Estado == 1)
            .OrderBy(c => c.Descripcion)
            .ToListAsync();

        CategoriasPagina = await _context.CategoriasPagina
            .Where(c => c.Estado == 1)
            .OrderBy(c => c.Descripcion)
            .ToListAsync();

        SubcategoriasPagina = await _context.SubcategoriasPagina
            .Where(s => s.Estado == 1)
            .OrderBy(s => s.Descripcion)
            .ToListAsync();

        Marcas = await _context.Marcas
            .Where(m => m.Estado == 1)
            .OrderBy(m => m.Descripcion)
            .ToListAsync();

        Medidas = await _context.Medidas
            .Where(m => m.Estado == 2)
            .OrderBy(m => m.Descripcion)
            .ToListAsync();
    }

    // ============================================================
    // 📌 HANDLERS AJAX
    // ============================================================
    public async Task<IActionResult> OnGetValidarCodbarraAsync(string codbarra, int productoId)
    {
        if (string.IsNullOrEmpty(codbarra))
            return new JsonResult(new { existe = false });

        var existe = await _context.Productos
            .AnyAsync(p => p.Codbarra == codbarra && p.IdProducto != productoId);

        return new JsonResult(new { existe });
    }

    public async Task<IActionResult> OnGetBuscarSubcategoriasAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            var subcategoriasDefault = await _context.Subcategorias
                .Where(s => s.Estado == 1)
                .OrderBy(s => s.Descripcion)
                .Take(20)
                .Select(s => new { id = s.IdSubcategoria, text = $"{s.IdSubcategoria} - {s.Descripcion}" })
                .ToListAsync();

            return new JsonResult(new { results = subcategoriasDefault });
        }

        var subcategorias = await _context.Subcategorias
            .Where(s => s.Estado == 1 &&
                        (s.IdSubcategoria.ToString().Contains(term) ||
                         (s.Descripcion != null && s.Descripcion.Contains(term))))
            .OrderBy(s => s.Descripcion)
            .Take(20)
            .Select(s => new { id = s.IdSubcategoria, text = $"{s.IdSubcategoria} - {s.Descripcion}" })
            .ToListAsync();

        return new JsonResult(new { results = subcategorias });
    }

    public async Task<IActionResult> OnGetObtenerSubcategoriaAsync(int id)
    {
        var subcategoria = await _context.Subcategorias
            .Where(s => s.IdSubcategoria == id && s.Estado == 1)
            .Select(s => new { id = s.IdSubcategoria, text = $"{s.IdSubcategoria} - {s.Descripcion}" })
            .FirstOrDefaultAsync();

        return new JsonResult(subcategoria);
    }

    // 🔽 FILTRO EN CASCADA: CATEGORÍAS PÁGINA POR CATEGORÍA PROPIA
    public async Task<IActionResult> OnGetCategoriasPaginaPorCategoriaPropiaAsync(int idCategoriaPropia)
    {
        var categoriasPagina = await _context.CategoriasPagina
            .Where(cp => cp.IdCategoriapropia == idCategoriaPropia && cp.Estado == 1)
            .OrderBy(cp => cp.Descripcion)
            .Select(cp => new { id = cp.IdCategoriapagina, descripcion = cp.Descripcion }) // ← nombres en minúscula
            .ToListAsync();

        return new JsonResult(categoriasPagina);
    }

    // 🔽 FILTRO EN CASCADA: SUBCATEGORÍAS PÁGINA POR CATEGORÍA PÁGINA
    public async Task<IActionResult> OnGetSubcategoriasPaginaPorCategoriaPaginaAsync(int idCategoriaPagina)
    {
        var subcategoriasPagina = await _context.SubcategoriasPagina
            .Where(sp => sp.IdCategoriapagina == idCategoriaPagina && sp.Estado == 1)
            .OrderBy(sp => sp.Descripcion)
            .Select(sp => new { id = sp.IdSubcategoriapagina, descripcion = sp.Descripcion }) // ← nombres en minúscula
            .ToListAsync();

        return new JsonResult(subcategoriasPagina);
    }
}