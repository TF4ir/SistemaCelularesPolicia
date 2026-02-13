using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Models.ViewModels;
using SistemaCelularesPolicia.Recursos.Data;

namespace SistemaCelularesPolicia.Controllers
{
    // Solo el Admin puede tocar esto
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly SisCeluPoliC _context;

        public RolesController(SisCeluPoliC context)
        {
            _context = context;
        }

        // 1. LISTAR ROLES
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        // 2. CREAR O EDITAR (GET - Muestra el formulario)
        public async Task<IActionResult> Upsert(int? id)
        {
            var viewModel = new RolViewModel();
            var todosLosPermisos = await _context.Permisos.ToListAsync();

            if (id.HasValue && id > 0)
            {
                // MODO EDICIÓN: Cargamos el rol y sus permisos actuales
                var rol = await _context.Roles
                    .Include(r => r.IdPermisos) // Importante: Traer la relación
                    .FirstOrDefaultAsync(r => r.IdRol == id);

                if (rol == null) return NotFound();

                viewModel.IdRol = rol.IdRol;
                viewModel.NombreRol = rol.NombreRol;
                viewModel.Descripcion = rol.Descripcion;

                // Llenar checkboxes (marcar los que ya tiene)
                viewModel.Permisos = todosLosPermisos.Select(p => new PermisoCheck
                {
                    IdPermiso = p.IdPermiso,
                    Nombre = p.NombrePermiso,
                    Modulo = p.Modulo,
                    Seleccionado = rol.IdPermisos.Any(rp => rp.IdPermiso == p.IdPermiso)
                }).ToList();
            }
            else
            {
                // MODO CREACIÓN: Todo vacío
                viewModel.Permisos = todosLosPermisos.Select(p => new PermisoCheck
                {
                    IdPermiso = p.IdPermiso,
                    Nombre = p.NombrePermiso,
                    Modulo = p.Modulo,
                    Seleccionado = false
                }).ToList();
            }

            return View(viewModel);
        }

        // 3. GUARDAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(RolViewModel model)
        {
            if (ModelState.IsValid)
            {
                Role rol;

                if (model.IdRol == 0)
                {
                    // CREAR NUEVO
                    rol = new Role
                    {
                        NombreRol = model.NombreRol,
                        Descripcion = model.Descripcion
                    };
                    _context.Roles.Add(rol);
                }
                else
                {
                    // EDITAR EXISTENTE
                    rol = await _context.Roles
                        .Include(r => r.IdPermisos) // Cargar para poder borrar los viejos
                        .FirstOrDefaultAsync(r => r.IdRol == model.IdRol);

                    rol.NombreRol = model.NombreRol;
                    rol.Descripcion = model.Descripcion;

                    // Limpiamos permisos anteriores para reescribirlos
                    rol.IdPermisos.Clear();
                }

                // Agregamos los permisos seleccionados
                var idsSeleccionados = model.Permisos.Where(x => x.Seleccionado).Select(x => x.IdPermiso).ToList();

                // Buscamos los objetos permiso en la BD
                var permisosAgregados = await _context.Permisos
                                        .Where(p => idsSeleccionados.Contains(p.IdPermiso))
                                        .ToListAsync();

                // Los vinculamos al rol (EF Core llena la tabla intermedia solo)
                foreach (var p in permisosAgregados)
                {
                    rol.IdPermisos.Add(p);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}