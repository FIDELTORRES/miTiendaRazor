using miTienda.Data;
using Microsoft.EntityFrameworkCore;
using Microting.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// 1. Servicios
builder.Services.AddRazorPages();

// 2. DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 4, 7));
builder.Services.AddDbContext<MiTiendaContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// 3. Sesión (DEBE ir ANTES de builder.Build())
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Construir la aplicación (SOLO UNA VEZ)
var app = builder.Build();

// 5. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 6. Sesión en el pipeline (después de UseRouting)
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();

app.Run();   // ← Solo una vez