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
    [Authorize(Roles = "Policia")] // Aseguramos que solo policías entren
    public class PoliciaController : Controller
    {
        private readonly ICelular _celularService;

        public PoliciaController(ICelular celularService)
        {
            _celularService = celularService;
        }

        // GET: Formulario de Registro
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            await CargarListasDesplegables();
            return View();
        }

        // POST: Guardar el Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Celular celular)
        {
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

            celular.FechaRegistro = DateTime.Now;

            // Si la fecha incautación viene vacía del form, usa la actual
            if (celular.FechaIncautacion == default)
            {
                celular.FechaIncautacion = DateTime.Now;
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
            var celular = await _celularService.ObtenerPorId(id);
            if (celular == null) return NotFound();

            await CargarListasDesplegables();
            return View(celular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Corregir(Celular celular)
        {
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
            var celular = await _celularService.ObtenerPorId(id);
            if (celular == null) return NotFound();

            // Solo necesitamos la lista de situaciones
            var situaciones = new List<string> { "INCAUTADO", "RECUPERADO", "BAJA", "DEVUELTO" };
            ViewBag.Situaciones = new SelectList(situaciones, celular.Situacion);

            return View(celular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idCelular, string nuevaSituacion, string justificacion)
        {
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
        public async Task<IActionResult> VerRegistros(int pagina = 1, string busqueda = "")
        {
            int registrosPorPagina = 10; // Cantidad fija por página

            // Pasamos la 'busqueda' al repositorio
            var modelo = await _celularService.ObtenerListadoPaginado(pagina, registrosPorPagina, busqueda);

            return View(modelo);
        }

        // Dashboard con estadísticas
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var datos = await _celularService.ObtenerDatosDashboard();
            return View(datos);
        }
    }
}