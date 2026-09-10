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

public class CrearModel : PageModel
{
    private readonly MiTiendaContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly string _rucEmisor = "10413820532";

    public CrearModel(MiTiendaContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public ProductoCreateViewModel Producto { get; set; } = new();

    // ============================================================
    // 📌 LISTAS PARA SELECTS
    // ============================================================
    public List<Subcategoria> Subcategorias { get; set; } = new();
    public List<Categoriapropia> CategoriasPropias { get; set; } = new();
    public List<Categoriapagina> CategoriasPagina { get; set; } = new();
    public List<Subcategoriapagina> SubcategoriasPagina { get; set; } = new();
    public List<Marca> Marcas { get; set; } = new();
    public List<Medida> Medidas { get; set; } = new();
    public List<Tienda> Tiendas { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CargarListasAsync();
        Producto.FechaIngreso = DateOnly.FromDateTime(DateTime.Now);
        Producto.Impuesto = 18.00m;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return Page();
        }

        string nombreImagen = string.Empty;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ============================================================
            // 1. PROCESAR LA IMAGEN
            // ============================================================
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

                nombreImagen = $"{Guid.NewGuid():N}{extension}";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "productos");
                Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, nombreImagen);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Producto.ImagenFile.CopyToAsync(fileStream);
                }
            }

            // ============================================================
            // 2. CREAR PRODUCTO PRINCIPAL
            // ============================================================
            var producto = new Producto
            {
                Codigo = Producto.Codigo,
                Codbarra = Producto.Codbarra,
                Nombre = Producto.Nombre,
                Descripcion = Producto.Descripcion,
                Imagen = nombreImagen,
                IdCategoria = Producto.IdSubcategoria,          // UNSPSC
                IdPresentacion = 0,
                IdSubcategoria = Producto.IdSubcategoriaPagina, // 🔽 Subcategoría de página (Nivel 3)
                CategoriaPaginaId = Producto.CategoriaPaginaId, // 🔽 Categoría de página (Nivel 2)
                Categorias = Producto.Categorias,               // 🔽 Categoría propia (Nivel 1)
                IdMedidas = Producto.IdMedidas,
                IdMarca = Producto.IdMarca,
                StockMinimo = Producto.StockMinimo,
                StockMaximo = Producto.StockMaximo,
                MargenPorcentaje = 0,
                IdTipoAfectacion = Producto.IdTipoAfectacion ?? "10",
                IdTipoFactura = Producto.IdTipoFactura ?? "0101",
                IdCodigoDetalle = Producto.IdCodigoDetalle ?? "01",
                IdTipoTributo = Producto.IdTipoTributo ?? "1000",
                IdTipoValorVenta = Producto.IdTipoValorVenta ?? "01",
                Impuesto = Producto.Impuesto ?? 18.00m,
                RucEmisor = _rucEmisor,
                FechaIngreso = Producto.FechaIngreso ?? DateOnly.FromDateTime(DateTime.Now),
                Estado = Producto.Estado ?? 1,
                FlagComprobante = 0,
                NombreArchi = Producto.ImagenFile?.FileName
            };
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // ============================================================
            // 3. OBTENER TIENDAS ACTIVAS (estado = 2)
            // ============================================================
            var tiendasActivas = await _context.Tiendas
                .Where(t => t.Estado == 2)
                .Select(t => t.IdLocal)
                .ToListAsync();

            if (!tiendasActivas.Any())
            {
                tiendasActivas = new List<int> { 2, 3 };
            }

            // ============================================================
            // 4. CREAR PRODUCTO EN TIENDA Y KARDEX (POR CADA TIENDA)
            // ============================================================
            foreach (var tiendaId in tiendasActivas)
            {
                var productoTienda = new ProductoTienda
                {
                    IdProducto = producto.IdProducto,
                    IdTienda = tiendaId,
                    Codbarra = Producto.Codbarra,
                    RucEmisor = _rucEmisor,
                    Stock = 0,
                    Estado = Producto.Estado ?? 1,
                    UsuarioRegistro = User.Identity?.Name ?? "Sistema",
                    FechaRegistro = DateTime.Now,
                    FechaModifica = DateOnly.FromDateTime(DateTime.Now),
                    HoraModifica = TimeOnly.FromDateTime(DateTime.Now),
                    SubcategoriaId = Producto.IdSubcategoria,
                    MarcaId = Producto.IdMarca
                };

                _context.ProductosTienda.Add(productoTienda);
                await _context.SaveChangesAsync();

                var kardex = new Kardex
                {
                    IdProducto = Producto.Codigo,
                    ProductoId = producto.IdProducto,
                    Cantidad = 0,
                    CantidadSalida = 0,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    IdAlmacen = tiendaId,
                    IdLocal = tiendaId,
                    Estado = 3,
                    RucEmisor = _rucEmisor,
                    IdUsuario = User.Identity?.Name ?? "Sistema",
                    Observacion = $"Registro inicial de producto en tienda {tiendaId}",
                    IdFlag = 1
                };

                _context.Kardex.Add(kardex);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            TempData["Success"] = $"✅ Producto '{producto.Nombre}' creado exitosamente en {tiendasActivas.Count} tienda(s)";
            return RedirectToPage("/Admin/Productos/Productos", new { tiendaFiltro = Producto.IdTienda });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (!string.IsNullOrEmpty(nombreImagen))
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "productos");
                string filePath = Path.Combine(uploadsFolder, nombreImagen);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            TempData["Error"] = $"❌ Error al crear el producto: {ex.Message}";
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

        Tiendas = await _context.Tiendas
            .Where(t => t.Estado == 2)
            .OrderBy(t => t.IdLocal)
            .ToListAsync();
    }

    // ============================================================
    // 📌 HANDLERS AJAX
    // ============================================================
    public async Task<IActionResult> OnGetValidarCodbarraAsync(string codbarra)
    {
        if (string.IsNullOrEmpty(codbarra))
            return new JsonResult(new { existe = false });

        var existe = await _context.Productos
            .AnyAsync(p => p.Codbarra == codbarra);

        return new JsonResult(new { existe });
    }

    public async Task<IActionResult> OnGetGenerarCodigoAsync(int idsubcategoria)
    {
        if (idsubcategoria <= 0)
            return new JsonResult(new { codigo = "" });

        var prefijo = idsubcategoria.ToString();
        var ultimo = await _context.Productos
            .Where(p => p.Codigo != null && p.Codigo.StartsWith(prefijo))
            .OrderByDescending(p => p.IdProducto)
            .Select(p => p.Codigo)
            .FirstOrDefaultAsync();

        string nuevoCodigo;
        if (!string.IsNullOrEmpty(ultimo))
        {
            var numStr = ultimo.Substring(prefijo.Length);
            if (int.TryParse(numStr, out int num))
            {
                nuevoCodigo = $"{prefijo}{(num + 1).ToString("D4")}";
            }
            else
            {
                nuevoCodigo = $"{prefijo}0001";
            }
        }
        else
        {
            nuevoCodigo = $"{prefijo}0001";
        }

        return new JsonResult(new { codigo = nuevoCodigo });
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

    // 🔽 HANDLER PARA FILTRO EN CASCADA: CATEGORÍAS PÁGINA POR CATEGORÍA PROPIA
    public async Task<IActionResult> OnGetCategoriasPaginaPorCategoriaPropiaAsync(int idCategoriaPropia)
    {
        var categoriasPagina = await _context.CategoriasPagina
            .Where(cp => cp.IdCategoriapropia == idCategoriaPropia && cp.Estado == 1)
            .OrderBy(cp => cp.Descripcion)
            .Select(cp => new { id = cp.IdCategoriapagina, descripcion = cp.Descripcion }) // ← nombres en minúscula
            .ToListAsync();

        return new JsonResult(categoriasPagina);
    }

    // 🔽 HANDLER PARA FILTRO EN CASCADA: SUBCATEGORÍAS PÁGINA POR CATEGORÍA PÁGINA
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