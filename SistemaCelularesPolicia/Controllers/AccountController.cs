using Google.Authenticator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Models.ViewModels;
using SistemaCelularesPolicia.Recursos;
using SistemaCelularesPolicia.Servicios.Interfaces;
using System.Security.Claims;

namespace SistemaCelularesPolicia.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioPublico _usuarioService;
        private readonly IPersonalPolicial _personalService;
        private readonly IEmail _emailService;

        // Inyectamos el repositorio
        public AccountController(IUsuarioPublico usuarioService, IPersonalPolicial personalService, IEmail emailservice)
        {
            _usuarioService = usuarioService;
            _personalService = personalService;
            _emailService = emailservice;
        }

        // --- VISTAS GET (Pantallas) ---
        public IActionResult Opciones() => View(); // O redirecciona a Home/Index si prefieres

        [HttpGet]
        public IActionResult LoginPolicia() => View();

        [HttpGet]
        public IActionResult LoginPublico() => View();

        [HttpGet]
        public IActionResult RegisterPublico() => View();

        [HttpGet]
        public IActionResult RegisterPolicia() => View();


        // --- REGISTRO PÚBLICO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPublic(RegisterPublicoViewModel viewModel)
        {
            // VALIDACIÓN AUTOMÁTICA
            if (!ModelState.IsValid) return View("RegisterPublico", viewModel);

            // VALIDAR DUPLICADOS (Lógica de Negocio)
            if (await _usuarioService.ExisteUsuario(viewModel.Email, viewModel.Dni))
            {
                ModelState.AddModelError("", "El correo o DNI ya están registrados en el sistema.");
                return View("RegisterPublico", viewModel);
            }

            // Generar Código de 6 dígitos para la verificación del email
            string codigo = new Random().Next(100000, 999999).ToString();

            // MAPEO (ViewModel -> Entidad BD)
            var nuevoUsuario = new UsuarioPublico
            {
                Dni = viewModel.Dni,
                Nombres = viewModel.Nombres,
                Apellidos = viewModel.Apellidos,
                Email = viewModel.Email,
                Telefono = viewModel.Telefono,
                Direccion = viewModel.Direccion,
                FechaNacimiento = viewModel.FechaNacimiento,

                // Encriptamos la clave del ViewModel
                ContraseñaHash = Utilidades.EncriptarClave(viewModel.Password),

                // Valores por defecto
                Activo = true,
                FechaRegistro = DateTime.Now,
                EmailVerificado = false,
                UltimoAcceso = DateTime.Now,

                // CÓDIGO Email:
                CodVerificacionEmail = codigo,
                FechaExpiracionCod = DateTime.Now.AddMinutes(10) // Expira en 10 min
            };

            // GUARDAR
            bool resultado = await _usuarioService.RegistrarUsuario(nuevoUsuario);

            if (resultado)
            {
                // 1. Enviar el correo
                await _emailService.EnviarCorreoVerificacion(nuevoUsuario.Email, nuevoUsuario.Nombres, codigo);

                // 2. Avisar al usuario que revise su bandeja
                TempData["MensajeInfo"] = "Registro iniciado. Hemos enviado un código a su correo.";

                // 3. Redirigir a la pantalla de poner el código
                // Pasamos el email como parámetro para que la vista sepa a quién validar
                return RedirectToAction("VerificarCodigo", new { email = viewModel.Email });
            }
            else
            {
                ModelState.AddModelError("", "Ocurrió un error al registrarse. Intente nuevamente.");
                return View("RegisterPublico", viewModel);
            }
        }

        [HttpGet]
        public IActionResult VerificarCodigo(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarCodigo(string email, string codigo)
        {
            // 1. Buscar usuario por email
            var usuario = await _usuarioService.ObtenerPorEmail(email);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                return View();
            }

            // 2. Validar si ya está verificado
            if (usuario.EmailVerificado == true)
            {
                return RedirectToAction("LoginPublico");
            }

            // 3. VALIDAR CÓDIGO Y EXPIRACIÓN
            if (usuario.CodVerificacionEmail != codigo)
            {
                ViewBag.Error = "Código incorrecto.";
                ViewBag.Email = email;
                return View();
            }

            if (usuario.FechaExpiracionCod < DateTime.Now)
            {
                ViewBag.Error = "El código ha expirado. Solicite uno nuevo.";
                ViewBag.Email = email;
                return View();
            }

            // 4. ÉXITO: Activar cuenta
            usuario.EmailVerificado = true;
            usuario.CodVerificacionEmail = null; // Limpiar código por seguridad
            usuario.FechaExpiracionCod = null;

            await _usuarioService.ActualizarUsuario(usuario);

            TempData["MensajeExito"] = "Cuenta verificada exitosamente. Ya puede iniciar sesión.";
            return RedirectToAction("LoginPublico");
        }

        // --- LOGIN PÚBLICO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPublico(string email, string password)
        {
            var usuario = await _usuarioService.ObtenerPorEmail(email);

            if (usuario != null)
            {
                // Verificar contraseña con BCrypt
                bool esValida = Utilidades.VerificarClave(password, usuario.ContraseñaHash);

                if (esValida)
                {
                    if (usuario.Activo == false)
                    {
                        ViewBag.Error = "Su cuenta está inactiva.";
                        return View();
                    }

                    if (usuario.EmailVerificado == false)
                    {
                        ViewBag.Error = "Su cuenta no está verificada. Revise su correo.";
                        return View("VerificarCodigo");
                    }

                    // --- CREAR LA IDENTIDAD (COOKIE) ---

                    // 1. Definir los datos que queremos guardar en la cookie (Claims)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Nombres), // Nombre para mostrar "Hola, Juan"
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim("IdUsuario", usuario.IdUsuarioPublico.ToString()), // Guardamos el ID para consultas
                        new Claim(ClaimTypes.Role, "Ciudadano") // Rol importante para autorización
                    };

                    // 2. Crear la identidad basada en esos claims
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // 3. Propiedades adicionales (ej: "Recordarme")
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // Mantiene la sesión aunque cierre el navegador
                    };

                    // 4. FIRMAR EL INGRESO (Esto crea y envía la cookie encriptada al navegador)
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // 5. Redirigir
                    return RedirectToAction("Index", "Consulta");
                }
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        // --- CERRAR SESIÓN (LOGOUT) ---
        public async Task<IActionResult> Salir()
        {
            // Borra la cookie del navegador
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al inicio
            return RedirectToAction("Index", "Home");
        }

        // --- LOGIN POLICIA ---
        [HttpGet]
        public IActionResult LoginPoliciaVerification()
        {
            // Si ya existe una cookie de sesión válida y es Policía, redirigir al Dashboard
            if (User.Identity.IsAuthenticated && User.IsInRole("Policia"))
            {
                return RedirectToAction("Registrar", "Policia"); // O la vista principal del policía
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPolicia(string email, string password)
        {
            // 1. Buscar policía por email (o código)
            var policia = await _personalService.ObtenerPorEmailOCodigo(email);

            if (policia != null)
            {
                // 2. Verificar password (BCrypt)
                bool passValida = Utilidades.VerificarClave(password, policia.ContrasenaHash);

                if (passValida)
                {
                    if (policia.Activo == false)
                    {
                        ViewBag.Error = "Su cuenta está inactiva.";
                        return View();
                    }

                    // --- AQUÍ EMPIEZA LA LÓGICA 2FA ---
                    if (policia.DosFactoresActivo == true)
                    {
                        // CASO A: TIENE 2FA ACTIVADO
                        // NO firmamos la cookie todavía.
                        // Guardamos el ID temporalmente para la siguiente pantalla.
                        TempData["PreAuthIdPolicia"] = policia.IdPolicial;

                        // Redirigir a pantalla intermedia de verificación
                        return RedirectToAction("Verificar2FA");
                    }
                    else
                    {
                        // CASO B: NO TIENE 2FA (Primer ingreso o desactivado)
                        // 1. Firmamos la cookie (Login normal)
                        await CrearCookieSesion(policia);

                        // 2. Lo OBLIGAMOS a configurar 2FA redirigiéndolo ahí
                        return RedirectToAction("Configurar2FA", "Seguridad");
                    }
                }
            }

            ViewBag.Error = "Credenciales incorrectas";
            return View();
        }

        // Pantalla Intermedia: GET
        [HttpGet]
        public IActionResult Verificar2FA()
        {
            // Validamos que venimos del Login con un ID temporal
            if (TempData["PreAuthIdPolicia"] == null)
            {
                return RedirectToAction("LoginPolicia");
            }

            // IMPORTANTE: Keep mantiene el dato para que no se borre al mostrar la vista
            TempData.Keep("PreAuthIdPolicia");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Verificar2FA(string codigo2fa)
        {
            if (TempData["PreAuthIdPolicia"] == null) return RedirectToAction("LoginPolicia");

            int idPolicia = (int)TempData["PreAuthIdPolicia"];
            TempData.Keep("PreAuthIdPolicia");

            var policia = await _personalService.ObtenerPorId(idPolicia); // Asegúrate de usar el método que trae el repositorio

            // MEJORA DE SEGURIDAD: Ventana de Bloqueo Temporal (Anti-Replay Robusto)
            // Si el usuario validó un código hace menos de 60 segundos, bloqueamos el intento.
            // Esto evita que usen el código actual de nuevo, O que usen un código anterior (A)
            // inmediatamente después de usar uno nuevo (B).
            if (policia.FechaUltimoCodigo.HasValue)
            {
                var segundosDesdeUltimoUso = DateTime.Now.Subtract(policia.FechaUltimoCodigo.Value).TotalSeconds;

                // 60 segundos es un buen balance (cubre la ventana actual y la anterior de tolerancia)
                if (segundosDesdeUltimoUso < 60)
                {
                    ViewBag.Error = "Por seguridad, debe esperar unos segundos antes de volver a ingresar otro código.";
                    // Opcional: Mandarlo al login para que no spamee intentos
                    return View();
                }
            }

            bool esValido = false;
            bool usoCodigoRespaldo = false;

            // INTENTO 1: TOTP (Google Authenticator)
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            try
            {
                esValido = tfa.ValidateTwoFactorPIN(policia.SecretKey2fa, codigo2fa);
            }
            catch { esValido = false; }

            // INTENTO 2: Códigos de Respaldo
            if (!esValido && !string.IsNullOrEmpty(policia.CodigosRespaldo2fa))
            {
                var listaCodigos = policia.CodigosRespaldo2fa.Split(',').ToList();
                if (listaCodigos.Contains(codigo2fa))
                {
                    esValido = true;
                    usoCodigoRespaldo = true;

                    listaCodigos.Remove(codigo2fa);
                    string nuevosCodigosStr = string.Join(",", listaCodigos);
                    await _personalService.ActualizarCodigosRespaldo(idPolicia, nuevosCodigosStr);
                }
            }

            if (esValido)
            {
                // SI ES VÁLIDO Y ES TOTP (No respaldo), REGISTRAMOS QUE YA SE USÓ
                if (!usoCodigoRespaldo)
                {
                    await _personalService.RegistrarUsoCodigo2fa(idPolicia, codigo2fa);
                }

                await CrearCookieSesion(policia);
                TempData.Remove("PreAuthIdPolicia");

                if (usoCodigoRespaldo)
                {
                    TempData["MensajeAlerta"] = "Ingresó usando un código de respaldo. Este código ya no se puede volver a usar.";
                }

                return RedirectToAction("Registrar", "Policia");
            }
            else
            {
                ViewBag.Error = "El código ingresado es incorrecto o ya expiró.";
                return View();
            }
        }

        // Método helper para no repetir código de cookie
        private async Task CrearCookieSesion(PersonalPolicial policia)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, policia.Nombres),
                new Claim(ClaimTypes.Email, policia.EmailInstitucional),
                new Claim("IdUsuario", policia.IdPolicial.ToString()),
                new Claim(ClaimTypes.Role, "Policia")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPolice(RegisterPoliciaViewModel viewModel)
        {
            // 1. VALIDACIÓN AUTOMÁTICA (Data Annotations)
            if (!ModelState.IsValid) return View("RegisterPolicia", viewModel);

            // 2. VALIDAR DUPLICADOS (Lógica de Negocio)
            if (await _personalService.ExistePolicia(viewModel.CodigoPolicial, viewModel.Dni, viewModel.EmailInstitucional))
            {

                ModelState.AddModelError("", "El Código Policial, DNI o Email ya están registrados en el sistema.");
                return View("RegisterPolicia", viewModel);
            }

            string codigo = new Random().Next(100000, 999999).ToString();

            // 3. MAPEO (ViewModel -> Entidad de BD)
            var nuevoPolicia = new PersonalPolicial
            {
                // Datos del formulario
                CodigoPolicial = viewModel.CodigoPolicial,
                Dni = viewModel.Dni,
                Nombres = viewModel.Nombres,
                Apellidos = viewModel.Apellidos,
                RangoGrado = viewModel.RangoGrado,
                UnidadDependencia = viewModel.UnidadDependencia,
                EmailInstitucional = viewModel.EmailInstitucional,
                TelefonoContacto = viewModel.TelefonoContacto,

                // Datos de seguridad y auditoría (El usuario no los controla)
                ContrasenaHash = Utilidades.EncriptarClave(viewModel.Password), // Encriptamos la clave del VM
                Activo = true,
                FechaRegistro = DateTime.Now,
                DosFactoresActivo = false,
                IntentosFallidos2fa = 0,

                // Código de verificación de email
                EmailVerificado = false,
                CodVerificacionEmail = codigo,
                FechaExpiracionCod = DateTime.Now.AddMinutes(10)
            };

            // 4. GUARDAR EN BASE DE DATOS
            bool resultado = await _personalService.RegistrarPolicia(nuevoPolicia);

            if (resultado)
            {
                // 1. Enviar Correo
                await _emailService.EnviarCorreoVerificacion(nuevoPolicia.EmailInstitucional, nuevoPolicia.Nombres, codigo);

                // 2. Redirigir a Verificación
                TempData["MensajeInfo"] = "Revise su correo institucional para verificar su cuenta.";
                return RedirectToAction("VerificarCodigoPolicia", new { email = viewModel.EmailInstitucional });
            }
            else
            {
                ModelState.AddModelError("", "Error al registrar.");
                return View("RegisterPolicia", viewModel);
            }
        }

        [HttpGet]
        public IActionResult VerificarCodigoPolicia(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarCodigoPolicia(string email, string codigo)
        {
            // 1. Buscar Policía
            var policia = await _personalService.ObtenerPorEmailOCodigo(email);

            if (policia == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                ViewBag.Email = email;
                return View();
            }

            // 2. Validaciones
            if (policia.EmailVerificado == true) return RedirectToAction("LoginPolicia");

            if (policia.CodVerificacionEmail != codigo)
            {
                ViewBag.Error = "Código incorrecto.";
                ViewBag.Email = email;
                return View();
            }

            if (policia.FechaExpiracionCod < DateTime.Now)
            {
                ViewBag.Error = "El código ha expirado.";
                ViewBag.Email = email;
                return View();
            }

            // 3. Activar (UPDATE)
            policia.EmailVerificado = true;
            policia.CodVerificacionEmail = null;
            policia.FechaExpiracionCod = null;

            await _personalService.ActualizarPolicia(policia);

            TempData["MensajeExito"] = "Cuenta verificada. Inicie sesión.";
            return RedirectToAction("LoginPolicia");
        }
    }
}