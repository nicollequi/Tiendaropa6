using Microsoft.EntityFrameworkCore;
using TiendaRopa.Models;

namespace TiendaRopa.Data
{
    public class TiendaContext : DbContext
    {
        public TiendaContext(DbContextOptions<TiendaContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Venta> Ventas { get; set; }
    }
}