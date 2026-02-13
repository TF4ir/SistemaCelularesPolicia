using Microsoft.AspNetCore.Mvc;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaCelularesPolicia.Controllers
{
    public class ConsultaController : Controller
    {
        private readonly ICelular _celularService;
        public ConsultaController(ICelular celularService)
        {
            _celularService = celularService;
        }
        public IActionResult Index()
        {
            return View();
        }

        // POST: Procesa la búsqueda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buscar(string imei)
        {
            if (string.IsNullOrEmpty(imei) || imei.Length != 15)
            {
                ViewBag.Error = "El IMEI debe tener exactamente 15 dígitos.";
                return View("Index");
            }

            // 1. Obtener ID del usuario logueado desde la Cookie
            var userIdString = User.FindFirst("IdUsuario")?.Value;
            int.TryParse(userIdString, out int userId);

            // 2. Registrar la auditoría de la consulta
            var nuevaConsulta = new ConsultaPublico
            {
                IdUsuarioPublico = userId,
                ImeiConsultado = imei,
                FechaConsulta = DateTime.UtcNow,
                IpConsulta = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            // 3. Buscar el celular
            var celularEncontrado = await _celularService.BuscarPorIMEI(imei);

            // Completar registro de auditoría
            if (celularEncontrado != null)
            {
                nuevaConsulta.Resultado = "ENCONTRADO";
                await _celularService.RegistrarConsulta(nuevaConsulta);

                // ENVIAR A LA VISTA DE RESULTADOS CON DATOS
                return View("Resultado", celularEncontrado);
            }
            else
            {
                nuevaConsulta.Resultado = "NO_ENCONTRADO";
                await _celularService.RegistrarConsulta(nuevaConsulta);

                // ENVIAR A LA VISTA DE RESULTADOS VACÍA (Significa no encontrado)
                return View("Resultado", null);
            }

        }
    }
}
