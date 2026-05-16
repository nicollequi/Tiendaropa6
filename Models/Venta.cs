using System.ComponentModel.DataAnnotations;

namespace TiendaRopa.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Range(1, 10000)]
        public int Cantidad { get; set; }

        public double PrecioUnitario { get; set; }

        public double Total => Cantidad * PrecioUnitario;
    }
}