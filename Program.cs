using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using miTienda.Data;
using miTienda.Models;
using miTienda.Services; 

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURAR DbContext CON MYSQL
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MiTiendaContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ============================================================
// 2. CONFIGURAR IDENTITY CON USUARIO_CLIENTE
// ============================================================
builder.Services.AddIdentity<UsuarioCliente, IdentityRole<int>>(options =>
{
    // ============================================================
    // 2.1 CONFIGURACIÓN DE USUARIO
    // ============================================================
    // El email será el username
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // ============================================================
    // 2.2 CONFIGURACIÓN DE CONTRASEÑAS
    // ============================================================
    options.Password.RequireDigit = true;              // Debe tener al menos un número
    options.Password.RequireLowercase = true;          // Debe tener al menos una minúscula
    options.Password.RequireUppercase = true;          // Debe tener al menos una mayúscula
    options.Password.RequireNonAlphanumeric = false;   // No requiere caracteres especiales
    options.Password.RequiredLength = 6;               // Longitud mínima: 6 caracteres

    // ============================================================
    // 2.3 CONFIGURACIÓN DE LOGIN
    // ============================================================
    options.SignIn.RequireConfirmedEmail = false;      // No requiere confirmación de email
    options.SignIn.RequireConfirmedPhoneNumber = false; // No requiere confirmación de teléfono

    // ============================================================
    // 2.4 CONFIGURACIÓN DE BLOQUEO
    // ============================================================
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Tiempo de bloqueo: 15 minutos
    options.Lockout.MaxFailedAccessAttempts = 5;       // Máximo de intentos fallidos: 5
    options.Lockout.AllowedForNewUsers = true;         // Bloqueo permitido para nuevos usuarios
})
.AddEntityFrameworkStores<MiTiendaContext>()           // Usar MiTiendaContext para almacenar datos
.AddDefaultTokenProviders();                           // Agregar proveedores de tokens por defecto

// ============================================================
// 3. CONFIGURAR COOKIES DE AUTENTICACIÓN
// ============================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;                    // Cookie solo accesible por HTTP
    options.ExpireTimeSpan = TimeSpan.FromDays(30);    // Cookie expira en 30 días
    options.LoginPath = "/Auth/Login";                 // Ruta de login
    options.LogoutPath = "/Auth/Logout";               // Ruta de logout
    options.AccessDeniedPath = "/Auth/AccessDenied";   // Ruta de acceso denegado
    options.SlidingExpiration = true;                  // Renovar cookie automáticamente
});

// ============================================================
// 4. CONFIGURAR SERVICIOS EXISTENTES
// ============================================================

// ✅ Session (para el carrito de compras)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);    // Sesión expira en 60 minutos
    options.Cookie.HttpOnly = true;                    // Cookie solo accesible por HTTP
    options.Cookie.IsEssential = true;                 // Cookie esencial para la aplicación
});

// ✅ Razor Pages
builder.Services.AddRazorPages();

// ✅ HttpContextAccessor (para acceder a HttpContext desde servicios)
builder.Services.AddHttpContextAccessor();
// ✅ REGISTRAR EL SERVICIO DE CORREO
builder.Services.AddScoped<IEmailService, EmailService>();
// ✅ AGREGAR: Registrar HttpClient y el servicio de WhatsApp
builder.Services.AddHttpClient();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
// ============================================================
// 5. CONSTRUIR LA APLICACIÓN
// ============================================================
var app = builder.Build();

// ============================================================
// 6. CONFIGURAR EL PIPELINE DE MIDDLEWARE
// ============================================================
// ✅ Cargar User Secrets en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ✅ Manejo de errores en desarrollo
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

// ✅ Redirección HTTPS
app.UseHttpsRedirection();

// ✅ Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// ✅ Enrutamiento
app.UseRouting();

// ============================================================
// 7. AUTENTICACIÓN Y AUTORIZACIÓN
// ============================================================
app.UseAuthentication();  // 🔐 Autenticación (Identity)
app.UseAuthorization();   // 🔐 Autorización (Roles, Políticas)

// ✅ Session (después de Authentication)
app.UseSession();

// ============================================================
// 8. MAPEAR LAS PÁGINAS RAZOR
// ============================================================
app.MapRazorPages();

// ============================================================
// 9. EJECUTAR LA APLICACIÓN
// ============================================================
app.Run();