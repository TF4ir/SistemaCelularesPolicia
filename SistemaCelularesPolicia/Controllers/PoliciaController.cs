using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;
using System.Security.Claims; // Necesario para User.FindFirst

namespace SistemaCelularesPolicia.Controllers
{
    [Authorize]
    public class PoliciaController : Controller
    {
        private readonly ICelular _celularService;
        private readonly SisCeluPoliC _context;

        public PoliciaController(ICelular celularService, SisCeluPoliC context)
        {
            _celularService = celularService;
            _context = context;
        }

        // GET: Formulario de Registro
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            if (!User.HasClaim("Permiso", "Celulares.Registrar")) return RedirectToAction("AccessDenied", "Home");

            await CargarListasDesplegables();
            return View();
        }

        // POST: Guardar el Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Celular celular)
        {
            if (!User.HasClaim("Permiso", "Celulares.Registrar")) return RedirectToAction("AccessDenied", "Home");
            // --- PASO 1: ASIGNAR DATOS AUTOMÁTICOS ---

            // Obtener ID del usuario logueado
            var claimId = User.FindFirst("IdUsuario");
            if (claimId != null && int.TryParse(claimId.Value, out int idPolicia))
            {
                celular.IdPolicialRegistro = idPolicia;
            }
            else
            {
                return RedirectToAction("LoginPolicia", "Account");
            }

            celular.FechaRegistro = DateTime.UtcNow;

            // Si la fecha incautación viene vacía del form, usa la actual
            if (celular.FechaIncautacion == default)
            {
                celular.FechaIncautacion = DateTime.UtcNow;
            }

            // --- PASO 2: CORREGIR EL MODELSTATE ---

            ModelState.Remove("IdPolicialRegistro");
            ModelState.Remove("FechaRegistro");
            ModelState.Remove("FechaIncautacion");

            // IMPORTANTE: EF Core a veces valida las relaciones de navegación
            ModelState.Remove("IdPolicialRegistroNavigation");
            ModelState.Remove("IdFiscaliaNavigation");

            // --- PASO 3: VALIDAR Y GUARDAR ---

            if (ModelState.IsValid)
            {
                var resultado = await _celularService.RegistrarIncautacion(celular);

                if (resultado)
                {
                    TempData["Mensaje"] = "¡Celular registrado correctamente!";
                    return RedirectToAction(nameof(Registrar));
                }
                else
                {
                    ModelState.AddModelError("", "Ocurrió un error al guardar en la base de datos.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }

            // Recargar listas si falló
            await CargarListasDesplegables();
            return View(celular);
        }

        // Método auxiliar para cargar dropdowns
        private async Task CargarListasDesplegables()
        {
            var situaciones = new List<string> { "INCAUTADO", "RECUPERADO"};
            ViewBag.Situaciones = new SelectList(situaciones);
        }

        // Búsqueda AJAX de Fiscalías
        [HttpGet]
        public async Task<IActionResult> BuscarFiscalias(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            // Usamos el servicio/repositorio, NO el _context directo
            var listaFiscalias = await _celularService.BuscarFiscalias(term);

            // Transformamos al formato que Select2 entiende (id, text)
            var resultado = listaFiscalias.Select(f => new
            {
                id = f.IdFiscalia,
                text = f.NombreFiscalia
            });

            return Json(resultado);
        }


        [HttpGet]
        public async Task<IActionResult> Corregir(int id)
        {
            if (!User.HasClaim("Permiso", "Celulares.Editar")) return RedirectToAction("AccessDenied", "Home");

            var celular = await _celularService.ObtenerPorId(id);
            if (celular == null) return NotFound();

            await CargarListasDesplegables();
            return View(celular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Corregir(Celular celular)
        {
            if (!User.HasClaim("Permiso", "Celulares.Editar")) return RedirectToAction("AccessDenied", "Home");

            // Limpiamos validaciones de campos que no vienen en este form o no se tocan
            ModelState.Remove("IdPolicialRegistroNavigation");
            ModelState.Remove("IdFiscaliaNavigation");

            if (ModelState.IsValid)
            {
                var resultado = await _celularService.CorregirDatosBasicos(celular);
                if (resultado)
                {
                    TempData["Mensaje"] = "Datos del celular corregidos correctamente.";
                    return RedirectToAction("VerRegistros");
                }
            }

            await CargarListasDesplegables();
            return View(celular);
        }

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            if (!User.HasClaim("Permiso", "Celulares.CambiarSituacion")) return RedirectToAction("AccessDenied", "Home");

            var celular = await _celularService.ObtenerPorId(id);
            if (celular == null) return NotFound();

            // Solo necesitamos la lista de situaciones
            var situaciones = new List<string> { "INCAUTADO", "RECUPERADO", "DEVUELTO" };
            ViewBag.Situaciones = new SelectList(situaciones, celular.Situacion);

            return View(celular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idCelular, string nuevaSituacion, string justificacion)
        {
            if (!User.HasClaim("Permiso", "Celulares.CambiarSituacion")) return RedirectToAction("AccessDenied", "Home");

            if (string.IsNullOrEmpty(justificacion))
            {
                ModelState.AddModelError("justificacion", "Es obligatorio justificar el cambio de estado.");
                // (Aquí deberías recargar el modelo y la vista si falla)
            }

            int idPolicia = int.Parse(User.FindFirst("IdUsuario").Value);

            var resultado = await _celularService.CambiarSituacion(idCelular, nuevaSituacion, justificacion, idPolicia);

            if (resultado)
            {
                TempData["Mensaje"] = "Estado actualizado y registrado en el historial.";
                return RedirectToAction("VerRegistros");
            }

            TempData["Error"] = "No se pudo actualizar el estado.";
            return RedirectToAction("VerRegistros");
        }

        // Para ver el listado de celulares incautados / Ver Registros
        [HttpGet]
        public async Task<IActionResult> VerRegistros(int pagina = 1, string busqueda = "", DateTime? fechaInicio = null, DateTime? fechaFin = null, List<string> situaciones = null, int? idDependenciaFiltro = null)
        {
            if (!User.HasClaim("Permiso", "Celulares.Consultar")) return RedirectToAction("AccessDenied", "Home");

            int registrosPorPagina = 10;
            bool esAdmin = User.HasClaim("Permiso", "Admin.Dependencias"); // Verificamos si es superusuario
            int idPolicia = int.Parse(User.FindFirst("IdUsuario").Value);

            // DETERMINAMOS EL ALCANCE DE DATOS (Data Scoping)
            int? dependenciaFinal = idDependenciaFiltro;

            if (!esAdmin)
            {
                // Si no es admin, ignoramos el filtro de la vista y lo FORZAMOS a su propia dependencia
                var policia = await _context.PersonalPolicials.FindAsync(idPolicia);
                dependenciaFinal = policia?.IdDependencia;
            }
            else
            {
                // Si es admin, le mandamos la lista de todas las comisarías para que pueda filtrar a gusto
                ViewBag.Dependencias = new SelectList(await _context.Dependencias.Where(d => d.Activa == true).OrderBy(d => d.Nombre).ToListAsync(), "IdDependencia", "Nombre", idDependenciaFiltro);
            }

            ViewBag.EsAdmin = esAdmin;

            var modelo = await _celularService.ObtenerListadoPaginado(pagina, registrosPorPagina, busqueda, fechaInicio, fechaFin, situaciones, dependenciaFinal);

            return View(modelo);
        }

        // Dashboard con estadísticas
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!User.HasClaim("Permiso", "Dashboard.Ver")) return RedirectToAction("AccessDenied", "Home");

            // Verificamos si es Administrador Global
            bool esAdmin = User.HasClaim("Permiso", "Admin.Dependencias");
            int? idDependencia = null;

            if (!esAdmin)
            {
                // Si no es admin, averiguamos su comisaría para enviarla como candado
                int idPolicia = int.Parse(User.FindFirst("IdUsuario").Value);
                var policia = await _context.PersonalPolicials.FindAsync(idPolicia);
                idDependencia = policia?.IdDependencia;
            }

            var datos = await _celularService.ObtenerDatosDashboard(idDependencia);
            datos.EsAdmin = esAdmin; // Le pasamos a la vista quién es

            return View(datos);
        }
    }
}