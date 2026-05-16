using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiendaRopa.Data;
using TiendaRopa.Models;

namespace TiendaRopa.Controllers
{
    public class VentaController : Controller
    {
        private readonly TiendaContext _context;

        public VentaController(TiendaContext context)
        {
            _context = context;
        }

        private bool EsAdmin() =>
            HttpContext.Session.GetString("UsuarioRol") == "Administrador";

        private bool EstaLogueado() =>
            HttpContext.Session.GetString("UsuarioNombre") != null;

        // Lista de ventas — solo Administrador
        public async Task<IActionResult> Index()
        {
            if (!EsAdmin()) return RedirectToAction("Index", "Home");

            var ventas = await _context.Ventas
                .Include(v => v.Producto)
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        // Formulario registrar venta — cualquier usuario logueado
        public IActionResult Create()
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Cuenta");

            ViewBag.Productos = new SelectList(
                _context.Productos.Where(p => p.Stock > 0), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productoId, int cantidad)
        {
            if (!EstaLogueado()) return RedirectToAction("Login", "Cuenta");

            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null) return NotFound();

            // Validación de stock
            if (cantidad > producto.Stock)
            {
                TempData["Error"] = $"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles.";
                ViewBag.Productos = new SelectList(
                    _context.Productos.Where(p => p.Stock > 0), "Id", "Nombre");
                return View();
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var venta = new Venta
            {
                ProductoId = productoId,
                UsuarioId = usuarioId,
                Cantidad = cantidad,
                PrecioUnitario = producto.Precio,
                Fecha = DateTime.Now
            };

            // Descontar stock
            producto.Stock -= cantidad;

            _context.Add(venta);
            _context.Update(producto);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Venta registrada correctamente.";
            return RedirectToAction("Index", "Producto");
        }
    }
}