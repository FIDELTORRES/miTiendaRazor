using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using miTienda.Models;
using miTienda.Data;   // ← AGREGAR ESTA LÍNEA
using System.Collections.Generic;
using System.Linq;

namespace miTienda.Pages;

public class TestDbModel : PageModel
{
    private readonly MiTiendaContext _context;

    public TestDbModel(MiTiendaContext context)
    {
        _context = context;
    }

    public bool ConexionExitosa { get; set; }
    public int CantidadProductos { get; set; }
    public string? MensajeError { get; set; }
    public List<Producto> Productos { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            // Intentamos contar productos
            CantidadProductos = await _context.Productos.CountAsync();
            
            // Traemos los primeros 5 productos
            Productos = await _context.Productos
                .Take(5)
                .ToListAsync();
            
            ConexionExitosa = true;
        }
        catch (Exception ex)
        {
            ConexionExitosa = false;
            MensajeError = ex.Message;
        }
    }
}