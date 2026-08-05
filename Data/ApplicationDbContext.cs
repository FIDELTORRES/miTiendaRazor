using Microsoft.EntityFrameworkCore;

namespace miTienda.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aquí agregarás tus tablas más adelante
    }
}