// ================================================================
// 📌 IMPORTAR NAMESPACES NECESARIOS
// ================================================================
using miTienda.Models;                    // 📌 Modelos (Producto, ProductoTienda, MiTiendaContext)
using Microsoft.EntityFrameworkCore;       // 📌 Entity Framework Core
using Microting.EntityFrameworkCore.MySql; // 📌 Conector MySQL para .NET 10

// ================================================================
// 📌 CREAR LA APLICACIÓN
// ================================================================
var builder = WebApplication.CreateBuilder(args);

// ================================================================
// 📌 AGREGAR SERVICIOS AL CONTENEDOR
// ================================================================
builder.Services.AddRazorPages();          // 📌 Habilita Razor Pages

// ================================================================
// 📌 CONFIGURAR LA CONEXIÓN A MySQL
// ================================================================
// 📌 Obtener la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 📌 Especificar la versión de MySQL manualmente (8.4.7)
//    FORMATO: new Version(mayor, menor, parche)
var serverVersion = new MySqlServerVersion(new Version(8, 4, 7));

// 📌 Registrar MiTiendaContext como servicio
builder.Services.AddDbContext<MiTiendaContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// ================================================================
// 📌 CONSTRUIR LA APLICACIÓN
// ================================================================
var app = builder.Build();

// ================================================================
// 📌 CONFIGURAR EL PIPELINE DE MIDDLEWARE
// ================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");     // 📌 Página de error en producción
    app.UseHsts();                         // 📌 HSTS (seguridad)
}

app.UseHttpsRedirection();                // 📌 Redirige HTTP a HTTPS
app.UseStaticFiles();                     // 📌 Sirve archivos estáticos (CSS, JS, imágenes)
app.UseRouting();                         // 📌 Habilita el enrutamiento
app.UseAuthorization();                  // 📌 Habilita autorización
app.MapRazorPages();                      // 📌 Mapea las rutas de Razor Pages

// ================================================================
// 📌 EJECUTAR LA APLICACIÓN
// ================================================================
app.Run();                                 // 📌 Inicia el servidor
