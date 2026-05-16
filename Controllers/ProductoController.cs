using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiendaRopa.Data;
using TiendaRopa.Models;

namespace TiendaRopa.Controllers
{
    public class ProductoController : Controller
    {
        private readonly TiendaContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductoController(TiendaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();
            return View(productos);
        }

        public IActionResult Create()
        {
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            ModelState.Remove("ImagenArchivo");
            ModelState.Remove("ImagenUrl");

            if (ModelState.IsValid)
            {
                if (producto.ImagenArchivo != null && producto.ImagenArchivo.Length > 0)
                {
                    var carpeta = Path.Combine(_env.WebRootPath, "imagenes", "productos");
                    Directory.CreateDirectory(carpeta);
                    var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(producto.ImagenArchivo.FileName);
                    var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
                    using var stream = new FileStream(rutaCompleta, FileMode.Create);
                    await producto.ImagenArchivo.CopyToAsync(stream);
                    producto.ImagenUrl = "/imagenes/productos/" + nombreArchivo;
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre");
            return View(producto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            ModelState.Remove("ImagenArchivo");
            ModelState.Remove("ImagenUrl");

            if (ModelState.IsValid)
            {
                if (producto.ImagenArchivo != null && producto.ImagenArchivo.Length > 0)
                {
                    var carpeta = Path.Combine(_env.WebRootPath, "imagenes", "productos");
                    Directory.CreateDirectory(carpeta);
                    var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(producto.ImagenArchivo.FileName);
                    var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
                    using var stream = new FileStream(rutaCompleta, FileMode.Create);
                    await producto.ImagenArchivo.CopyToAsync(stream);
                    producto.ImagenUrl = "/imagenes/productos/" + nombreArchivo;
                }

                _context.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}