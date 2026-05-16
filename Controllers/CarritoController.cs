using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaRopa.Data;
using TiendaRopa.Models;
using System.Text.Json;

namespace TiendaRopa.Controllers
{
    public class CarritoController : Controller
    {
        private readonly TiendaContext _context;
        private const string CarritoKey = "Carrito";

        public CarritoController(TiendaContext context)
        {
            _context = context;
        }

        private List<CarritoItem> ObtenerCarrito()
        {
            var json = HttpContext.Session.GetString(CarritoKey);
            return json == null ? new List<CarritoItem>() : JsonSerializer.Deserialize<List<CarritoItem>>(json)!;
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            HttpContext.Session.SetString(CarritoKey, JsonSerializer.Serialize(carrito));
        }

        public IActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null) return NotFound();

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);
            int cantidadActual = item?.Cantidad ?? 0;

            // Validación de stock
            if (cantidadActual + cantidad > producto.Stock)
            {
                TempData["Error"] = $"Solo hay {producto.Stock} unidades disponibles de {producto.Nombre}.";
                return RedirectToAction("Index", "Producto");
            }

            if (item != null)
            {
                item.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = cantidad,
                    ImagenUrl = producto.ImagenUrl
                });
            }

            GuardarCarrito(carrito);
            TempData["Exito"] = $"{producto.Nombre} agregado al carrito.";
            return RedirectToAction("Index", "Producto");
        }

        [HttpPost]
        public IActionResult Eliminar(int productoId)
        {
            var carrito = ObtenerCarrito();
            carrito.RemoveAll(i => i.ProductoId == productoId);
            GuardarCarrito(carrito);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove(CarritoKey);
            return RedirectToAction("Index");
        }
    }
}