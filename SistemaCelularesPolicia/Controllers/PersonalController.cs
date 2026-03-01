using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectList
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Recursos.Data;

namespace SistemaCelularesPolicia.Controllers
{
    [Authorize]
    public class PersonalController : Controller
    {
        private readonly SisCeluPoliC _context;

        public PersonalController(SisCeluPoliC context)
        {
            _context = context;
        }

        // 1. LISTA DE PERSONAL
        public async Task<IActionResult> Index()
        {
            if (!User.HasClaim("Permiso", "Admin.Personal")) return RedirectToAction("AccessDenied", "Home");

            var personal = await _context.PersonalPolicials
                .Include(p => p.IdRolNavigation) // Trae el nombre de su rol actual
                .Where(p => p.Activo == true)
                .ToListAsync();

            return View(personal);
        }

        // 2. CAMBIAR ROL (GET - Muestra el modal o pagina)
        public async Task<IActionResult> AsignarRol(int id)
        {
            if (!User.HasClaim("Permiso", "Admin.Personal")) return RedirectToAction("AccessDenied", "Home");

            var policia = await _context.PersonalPolicials.FindAsync(id);
            if (policia == null) return NotFound();

            // Preparamos la lista desplegable de Roles
            ViewBag.ListaRoles = new SelectList(await _context.Roles.ToListAsync(), "IdRol", "NombreRol", policia.IdRol);

            return View(policia);
        }

        // 3. GUARDAR CAMBIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(int id, int nuevoIdRol)
        {
            if (!User.HasClaim("Permiso", "Admin.Personal")) return RedirectToAction("AccessDenied", "Home");

            var policia = await _context.PersonalPolicials.FindAsync(id);
            if (policia == null) return NotFound();

            // Actualizamos el rol
            policia.IdRol = nuevoIdRol;

            // Opcional: Desloguear al usuario remotamente (avanzado) o esperar a que su cookie expire
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = $"Rol actualizado correctamente para {policia.Nombres}";

            return RedirectToAction(nameof(Index));
        }
    }
}