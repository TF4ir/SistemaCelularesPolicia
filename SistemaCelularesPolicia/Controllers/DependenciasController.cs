using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Recursos.Data;

namespace SistemaCelularesPolicia.Controllers
{
    [Authorize] // Bloqueo general
    public class DependenciasController : Controller
    {
        private readonly SisCeluPoliC _context;

        public DependenciasController(SisCeluPoliC context)
        {
            _context = context;
        }

        // 1. LISTAR (INDEX)
        public async Task<IActionResult> Index(string busqueda)
        {
            // SEGURIDAD: Validamos permiso (Igual que en Roles)
            if (!User.HasClaim("Permiso", "Usuarios.Gestionar"))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var query = _context.Dependencias.AsQueryable();

            // Lógica del buscador
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(d => d.Nombre.Contains(busqueda) || d.Provincia.Contains(busqueda));
            }

            var lista = await query
                .OrderBy(d => d.Provincia) // Agrupar visualmente por provincia
                .ThenBy(d => d.Nombre)
                .ToListAsync();

            ViewData["BusquedaActual"] = busqueda;
            return View(lista);
        }

        // 2. CREAR O EDITAR (VISTA)
        public async Task<IActionResult> Upsert(int? id)
        {
            if (!User.HasClaim("Permiso", "Usuarios.Gestionar")) return RedirectToAction("AccessDenied", "Home");

            Dependencia dependencia = new Dependencia();

            if (id.HasValue && id > 0)
            {
                // Es edición
                dependencia = await _context.Dependencias.FindAsync(id);
                if (dependencia == null) return NotFound();
            }

            return View(dependencia);
        }

        // 3. GUARDAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Dependencia model)
        {
            if (!User.HasClaim("Permiso", "Usuarios.Gestionar")) return RedirectToAction("AccessDenied", "Home");

            if (ModelState.IsValid)
            {
                if (model.IdDependencia == 0)
                {
                    // Crear
                    _context.Dependencias.Add(model);
                    TempData["MensajeExito"] = "Unidad registrada correctamente.";
                }
                else
                {
                    // Editar
                    _context.Dependencias.Update(model);
                    TempData["MensajeExito"] = "Unidad actualizada correctamente.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}