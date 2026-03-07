using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SistemaCelularesPolicia.Servicios.Interfaces;

namespace SistemaCelularesPolicia.Filters
{
    public class Require2FAAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. SOLO interceptamos si está logueado y SU COOKIE TIENE EL SELLO POLICIAL
            if (user.Identity != null && user.Identity.IsAuthenticated && user.HasClaim("TipoCuenta", "Policial"))
            {
                var controller = context.RouteData.Values["controller"]?.ToString();

                // 2. EXCEPCIÓN: Si está en Seguridad, Account o Home, lo dejamos pasar para evitar bucles
                if (controller == "Seguridad" || controller == "Account" || controller == "Home")
                {
                    await next();
                    return;
                }

                var personalService = context.HttpContext.RequestServices.GetService<IPersonalPolicial>();
                var userIdClaim = user.FindFirst("IdUsuario")?.Value;

                if (int.TryParse(userIdClaim, out int idUser))
                {
                    // 3. CONSULTA BD: Verificamos si tiene el 2FA activo
                    var policia = await personalService.ObtenerPorId(idUser);

                    // 4. EL BLOQUEO: Si es false, lo mandamos al QR sin importar la vista que pidió
                    if (policia != null && policia.DosFactoresActivo == false)
                    {
                        context.Result = new RedirectToActionResult("Configurar2FA", "Seguridad", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}