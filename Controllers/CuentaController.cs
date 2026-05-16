using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaRopa.Data;
using TiendaRopa.Models;
using System.Security.Cryptography;
using System.Text;

namespace TiendaRopa.Controllers
{
    public class CuentaController : Controller
    {
        private readonly TiendaContext _context;

        public CuentaController(TiendaContext context)
        {
            _context = context;
        }

        private static string HashSHA256(string texto)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes).ToLower();
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string contrasena)
        {
            var hash = HashSHA256(contrasena);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Contrasena == hash);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Registro() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Usuarios
                    .AnyAsync(u => u.Email == usuario.Email);

                if (existe)
                {
                    ViewBag.Error = "Ya existe una cuenta con ese correo";
                    return View(usuario);
                }

                usuario.Contrasena = HashSHA256(usuario.Contrasena);
                usuario.Rol = "Cliente";
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(usuario);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}