using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Models;
using System;
using System.Threading.Tasks;

namespace miTienda.Pages;

public class EditarProductoModel : PageModel
{
    private readonly MiTiendaContext _context;

    public EditarProductoModel(MiTiendaContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProductoTienda ProductoTienda { get; set; } = new();

    [BindProperty]
    public Producto Producto { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Buscar el producto en tienda incluyendo el producto principal
        var productoTienda = await _context.ProductosTienda
            .Include(p => p.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productoTienda == null)
        {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage("/Productos");
        }

        // Cargar datos en las propiedades
        ProductoTienda = productoTienda;
        Producto = productoTienda.Producto ?? new Producto();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Actualizar Producto Principal
            var productoExistente = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == Producto.IdProducto);

            if (productoExistente == null)
            {
                TempData["Error"] = "Producto principal no encontrado";
                return Page();
            }

            // Actualizar campos del producto
            productoExistente.Nombre = Producto.Nombre;
            productoExistente.Descripcion = Producto.Descripcion;
            productoExistente.Imagen = Producto.Imagen;
            productoExistente.Codigo = Producto.Codigo;
            productoExistente.Estado = Producto.Estado;

            _context.Productos.Update(productoExistente);

            // 2. Actualizar ProductoTienda
            var productoTiendaExistente = await _context.ProductosTienda
                .FirstOrDefaultAsync(p => p.Id == ProductoTienda.Id);

            if (productoTiendaExistente == null)
            {
                TempData["Error"] = "Producto en tienda no encontrado";
                return Page();
            }

            // Actualizar campos de la tienda
            productoTiendaExistente.Codbarra = ProductoTienda.Codbarra;
            productoTiendaExistente.Stock = ProductoTienda.Stock;
            productoTiendaExistente.PrecioVenta = ProductoTienda.PrecioVenta;
            productoTiendaExistente.PrecioCompra = ProductoTienda.PrecioCompra;
            productoTiendaExistente.IdTienda = ProductoTienda.IdTienda;
            productoTiendaExistente.Estado = ProductoTienda.Estado;
            productoTiendaExistente.FechaModifica = DateOnly.FromDateTime(DateTime.Now);
            productoTiendaExistente.HoraModifica = TimeOnly.FromDateTime(DateTime.Now);

            _context.ProductosTienda.Update(productoTiendaExistente);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"✅ Producto '{Producto.Nombre}' actualizado correctamente";
            return RedirectToPage("/Productos", new
            {
                tiendaFiltro = ProductoTienda.IdTienda
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = $"❌ Error al actualizar: {ex.Message}";
            return Page();
        }
    }
}