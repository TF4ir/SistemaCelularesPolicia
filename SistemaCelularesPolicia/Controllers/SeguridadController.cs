using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaCelularesPolicia.Servicios.Interfaces;
using System.Security.Claims;
using System.Text;

namespace SistemaCelularesPolicia.Controllers
{
    [Authorize(Policy = "SoloPolicias")]
    public class SeguridadController : Controller
    {
        private readonly IPersonalPolicial _personalService;

        public SeguridadController(IPersonalPolicial personalService)
        {
            _personalService = personalService;
        }

        // 1. PANTALLA: Mostrar el código QR
        [HttpGet]
        public async Task<IActionResult> Configurar2FA()
        {
            // Obtener ID del usuario logueado
            int idUser = int.Parse(User.FindFirst("IdUsuario").Value);
            var usuario = await _personalService.ObtenerPorId(idUser);

            // Si ya lo tiene activo, no debería estar aquí (o mostrar opción de desactivar)
            if (usuario.DosFactoresActivo == true)
            {
                return RedirectToAction("Index", "Policia");
            }

            // Generar Llave Secreta Nueva (si no tiene una temporal)
            // Usamos TempData para persistir la llave entre el GET y el POST
            string secretKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10).ToUpper();
            TempData["SecretKeyTemp"] = secretKey;

            // Generar QR
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode(
                "Sistema PNP",           // Nombre App (Emisor)
                usuario.EmailInstitucional, // Identificador Usuario
                secretKey,               // Llave secreta
                false,                   // Usar base64 QR
                3                        // Tamaño QR (pixeles)
            );

            // Pasar datos a la vista
            ViewBag.QrCodeUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.ManualCode = setupInfo.ManualEntryKey;

            return View();
        }

        // 2. ACCIÓN: Validar el código que el usuario escaneó
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarActivacion(string codigoIngresado)
        {
            // Recuperar la llave secreta que generamos en el GET
            if (TempData["SecretKeyTemp"] == null)
            {
                // Si expiró la sesión o recargó, reiniciar proceso
                return RedirectToAction("Configurar2FA");
            }
            string secretKey = TempData["SecretKeyTemp"].ToString();
            TempData.Keep("SecretKeyTemp"); // Mantenerlo por si falla el código

            // Validar con Google Authenticator
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            bool esCorrecto = tfa.ValidateTwoFactorPIN(secretKey, codigoIngresado);

            if (esCorrecto)
            {
                int idUser = int.Parse(User.FindFirst("IdUsuario").Value);

                // Generar 10 códigos de respaldo aleatorios
                var codigosRespaldo = GenerarCodigosRespaldo();
                string codigosJson = string.Join(",", codigosRespaldo); // Guardar como CSV simple

                // GUARDAR EN BASE DE DATOS
                await _personalService.Activar2fa(idUser, secretKey, codigosJson);

                // Mostrar pantalla de éxito con los códigos de respaldo
                return View("PantallaCodigosRespaldo", codigosRespaldo);
            }
            else
            {
                ViewBag.Error = "El código ingresado es incorrecto. Intente nuevamente.";
                // Regenerar QR visualmente (requeriría volver a llamar a GenerateSetupCode o pasarlo de nuevo)
                return RedirectToAction("Configurar2FA");
            }
        }

        private List<string> GenerarCodigosRespaldo()
        {
            var codigos = new List<string>();
            var random = new Random();
            for (int i = 0; i < 10; i++)
            {
                codigos.Add(random.Next(100000, 999999).ToString());
            }
            return codigos;
        }
    }
}
