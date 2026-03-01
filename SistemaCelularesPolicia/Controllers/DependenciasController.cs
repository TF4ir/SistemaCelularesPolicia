using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Recursos.Data;

namespace SistemaCelularesPolicia.Controllers
{
    [Authorize]
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
            if (!User.HasClaim("Permiso", "Admin.Dependencias")) return RedirectToAction("AccessDenied", "Home");

            // Agregamos los Include para traer los datos de Región y División
            var query = _context.Dependencias
                .Include(d => d.IdRegionNavigation)
                .Include(d => d.IdDivisionNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(d => d.Nombre.Contains(busqueda) ||
                                         d.IdRegionNavigation.NombreRegion.Contains(busqueda) ||
                                         d.IdDivisionNavigation.NombreDivision.Contains(busqueda));
            }

            var lista = await query
                .OrderBy(d => d.IdRegionNavigation.NombreRegion)
                .ThenBy(d => d.IdDivisionNavigation.NombreDivision)
                .ThenBy(d => d.Nombre)
                .ToListAsync();

            ViewData["BusquedaActual"] = busqueda;
            return View(lista);
        }

        // 2. CREAR O EDITAR (VISTA GET)
        public async Task<IActionResult> Upsert(int? id)
        {
            if (!User.HasClaim("Permiso", "Admin.Dependencias")) return RedirectToAction("AccessDenied", "Home");

            Dependencia dependencia = new Dependencia();

            if (id.HasValue && id > 0)
            {
                dependencia = await _context.Dependencias.FindAsync(id);
                if (dependencia == null) return NotFound();
            }

            // Llenar ViewBag para Regiones (siempre se cargan todas las activas)
            ViewBag.Regiones = new SelectList(_context.RegionPolicials.Where(r => r.Activa == true), "IdRegion", "NombreRegion", dependencia.IdRegion);

            // Llenar ViewBag para Divisiones (solo si ya tiene una región seleccionada, útil para la Edición)
            var divisiones = dependencia.IdRegion.HasValue
                ? _context.DivisionPolicials.Where(d => d.IdRegion == dependencia.IdRegion && d.Activa == true).ToList()
                : new List<DivisionPolicial>();

            ViewBag.Divisiones = new SelectList(divisiones, "IdDivision", "NombreDivision", dependencia.IdDivision);

            return View(dependencia);
        }

        // 3. GUARDAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Dependencia model)
        {
            if (!User.HasClaim("Permiso", "Admin.Dependencias")) return RedirectToAction("AccessDenied", "Home");

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

            // Si hay error de validación, recargar los dropdowns
            ViewBag.Regiones = new SelectList(_context.RegionPolicials.Where(r => r.Activa == true), "IdRegion", "NombreRegion", model.IdRegion);
            var divisiones = model.IdRegion.HasValue ? _context.DivisionPolicials.Where(d => d.IdRegion == model.IdRegion && d.Activa == true).ToList() : new List<DivisionPolicial>();
            ViewBag.Divisiones = new SelectList(divisiones, "IdDivision", "NombreDivision", model.IdDivision);

            return View(model);
        }

        // 4. NUEVO: ENDPOINT PARA EL MENÚ EN CASCADA (AJAX)
        [HttpGet]
        public async Task<IActionResult> ObtenerDivisiones(int idRegion)
        {
            var divisiones = await _context.DivisionPolicials
                .Where(d => d.IdRegion == idRegion && d.Activa == true)
                .Select(d => new { value = d.IdDivision, text = d.NombreDivision })
                .ToListAsync();

            return Json(divisiones);
        }
    }
}