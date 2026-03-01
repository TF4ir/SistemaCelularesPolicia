using Google.Authenticator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Models.ViewModels;
using SistemaCelularesPolicia.Recursos;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;
using System.Security.Claims;

namespace SistemaCelularesPolicia.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioPublico _usuarioService;
        private readonly IPersonalPolicial _personalService;
        private readonly IEmail _emailService;
        private readonly SisCeluPoliC _context;

        // Inyectamos el repositorio
        public AccountController(IUsuarioPublico usuarioService, IPersonalPolicial personalService, IEmail emailservice, SisCeluPoliC context)
        {
            _usuarioService = usuarioService;
            _personalService = personalService;
            _emailService = emailservice;
            _context = context;
        }

        // --- VISTAS GET (Pantallas) ---
        public IActionResult Opciones() => View(); // O redirecciona a Home/Index si prefieres

        [HttpGet]
        public IActionResult LoginPolicia() => View();

        [HttpGet]
        public IActionResult LoginPublico() => View();

        [HttpGet]
        public IActionResult RegisterPublico() => View();

        // --- REGISTRO POLICÍA (GET) ---
        [HttpGet]
        public async Task<IActionResult> RegisterPolicia()
        {
            // Cargamos la lista de Regiones en lugar de Provincias
            ViewBag.Regiones = await _context.RegionPolicials
                .Where(r => r.Activa == true)
                .OrderBy(r => r.NombreRegion)
                .ToListAsync();

            return View();
        }

        // --- API PARA CARGAR MENÚS EN CASCADA (AJAX) ---
        [HttpGet]
        public async Task<JsonResult> ObtenerDivisionesPorRegion(int idRegion)
        {
            var divisiones = await _context.DivisionPolicials
                .Where(d => d.IdRegion == idRegion && d.Activa == true)
                .Select(d => new { valor = d.IdDivision, texto = d.NombreDivision })
                .ToListAsync();

            return Json(divisiones);
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerDependenciasPorDivision(int idDivision)
        {
            var dependencias = await _context.Dependencias
                .Where(d => d.IdDivision == idDivision && d.Activa == true)
                .Select(d => new { valor = d.IdDependencia, texto = d.Nombre })
                .ToListAsync();

            return Json(dependencias);
        }

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
                FechaRegistro = DateTime.UtcNow,
                EmailVerificado = false,
                UltimoAcceso = DateTime.UtcNow,

                // CÓDIGO Email:
                CodVerificacionEmail = codigo,
                FechaExpiracionCod = DateTime.UtcNow.AddMinutes(10) // Expira en 10 min
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

            if (usuario.FechaExpiracionCod < DateTime.UtcNow)
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

        [HttpGet]
        public IActionResult RecuperarPublico()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPublico(SolicitarRecuperacionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Buscar si el correo existe
            var usuario = await _usuarioService.ObtenerPorEmail(model.Email);

            if (usuario == null)
            {
                TempData["MensajeInfo"] = "Si el correo existe, se han enviado las instrucciones.";
                return View(model);
            }

            // 2. Generar Código y Guardar en BD (Update)
            string codigo = new Random().Next(100000, 999999).ToString();

            usuario.CodVerificacionEmail = codigo;
            usuario.FechaExpiracionCod = DateTime.UtcNow.AddMinutes(15);

            await _usuarioService.ActualizarUsuario(usuario);

            // 3. Enviar Correo
            await _emailService.EnviarCorreoVerificacion(usuario.Email, usuario.Nombres, codigo);

            // 4. Redirigir al Paso 2 (Llevando el email)
            return RedirectToAction("CambiarPasswordPublico", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult CambiarPasswordPublico(string email)
        {
            // Preparamos el modelo con el email recibido para que el usuario no tenga que escribirlo de nuevo
            var model = new RestablecerPasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPasswordPublico(RestablecerPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _usuarioService.ObtenerPorEmail(model.Email);

            // Validaciones
            if (usuario == null ||
                usuario.CodVerificacionEmail != model.Codigo ||
                usuario.FechaExpiracionCod < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "El código es inválido o ha expirado.");
                return View(model);
            }

            // CAMBIO DE CONTRASEÑA
            usuario.ContraseñaHash = Utilidades.EncriptarClave(model.NuevaPassword);

            // Limpiamos el código usado
            usuario.CodVerificacionEmail = null;
            usuario.FechaExpiracionCod = null;

            await _usuarioService.ActualizarUsuario(usuario);

            TempData["MensajeExito"] = "Contraseña actualizada. Inicie sesión.";
            return RedirectToAction("LoginPublico");
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
                var segundosDesdeUltimoUso = DateTime.UtcNow.Subtract(policia.FechaUltimoCodigo.Value).TotalSeconds;

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

        // Método helper actualizado para Roles Dinámicos
        private async Task CrearCookieSesion(PersonalPolicial policia)
        {
            // 1. Nombre del Rol (Usando IdRolNavigation)
            // Justo antes de leer el nombre del rol:
            var idRolNumerico = policia.IdRol;
            System.Diagnostics.Debug.WriteLine($"---> EL ID EN LA BD ES: {idRolNumerico}");
            string nombreRol = policia.IdRolNavigation?.NombreRol ?? "SinRol";

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, policia.Nombres),
                    new Claim(ClaimTypes.Email, policia.EmailInstitucional ?? ""),
                    new Claim("IdUsuario", policia.IdPolicial.ToString()),
                    new Claim(ClaimTypes.Role, nombreRol)
                };

            // 2. Cargar Permisos
            // Verificamos si el rol tiene la lista "IdPermisos" cargada
            if (policia.IdRolNavigation != null && policia.IdRolNavigation.IdPermisos != null)
            {
                // Recorremos directamente los permisos (no hay tabla intermedia visible)
                foreach (var permiso in policia.IdRolNavigation.IdPermisos)
                {
                    // Agregamos el nombre del permiso (ej: "Celulares.Registro")
                    claims.Add(new Claim("Permiso", permiso.NombrePermiso));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPolice(RegisterPoliciaViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Regiones = await _context.RegionPolicials.Where(r => r.Activa == true).ToListAsync();
                return View("RegisterPolicia", viewModel);
            }

            if (await _personalService.ExistePolicia(viewModel.CodigoPolicial, viewModel.Dni, viewModel.EmailInstitucional))
            {
                ModelState.AddModelError("", "El Código Policial, DNI o Email ya están registrados.");
                ViewBag.Regiones = await _context.RegionPolicials.Where(r => r.Activa == true).ToListAsync();
                return View("RegisterPolicia", viewModel);
            }

            string codigo = new Random().Next(100000, 999999).ToString();

            var rolPorDefecto = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Registrador");

            // TRUCO DE SEGURIDAD: Como tu columna antigua 'UnidadDependencia' no acepta nulos en BD, 
            // buscamos el nombre real para guardarlo ahí y no romper la BD, mientras guardamos el ID nuevo.
            var dependenciaSeleccionada = await _context.Dependencias.FindAsync(viewModel.IdDependencia);
            string nombreUnidad = dependenciaSeleccionada?.Nombre ?? "-";

            var nuevoPolicia = new PersonalPolicial
            {
                CodigoPolicial = viewModel.CodigoPolicial,
                Dni = viewModel.Dni,
                Nombres = viewModel.Nombres,
                Apellidos = viewModel.Apellidos,
                RangoGrado = viewModel.RangoGrado,
                IdDependencia = viewModel.IdDependencia,
                UnidadDependencia = nombreUnidad, // Mantenemos el campo string lleno por compatibilidad
                EmailInstitucional = viewModel.EmailInstitucional,
                TelefonoContacto = viewModel.TelefonoContacto,
                ContrasenaHash = Utilidades.EncriptarClave(viewModel.Password),
                Activo = true,
                FechaRegistro = DateTime.UtcNow,

                IdRol = rolPorDefecto?.IdRol,

                DosFactoresActivo = false,
                IntentosFallidos2fa = 0,
                EmailVerificado = false,
                CodVerificacionEmail = codigo,
                FechaExpiracionCod = DateTime.UtcNow.AddMinutes(10)
            };

            bool resultado = await _personalService.RegistrarPolicia(nuevoPolicia);

            if (resultado)
            {
                await _emailService.EnviarCorreoVerificacion(nuevoPolicia.EmailInstitucional, nuevoPolicia.Nombres, codigo);
                TempData["MensajeInfo"] = "Revise su correo institucional para verificar su cuenta.";
                return RedirectToAction("VerificarCodigoPolicia", new { email = viewModel.EmailInstitucional });
            }
            else
            {
                ModelState.AddModelError("", "Error al registrar.");
                ViewBag.Regiones = await _context.RegionPolicials.Where(r => r.Activa == true).ToListAsync();
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

            if (policia.FechaExpiracionCod < DateTime.UtcNow)
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

        [HttpGet]
        public IActionResult RecuperarPolicia()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPolicia(SolicitarRecuperacionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Buscar si el correo existe
            var policia = await _personalService.ObtenerPorEmailOCodigo(model.Email);

            if (policia == null)
            {
                TempData["MensajeInfo"] = "Si el correo existe, se han enviado las instrucciones.";
                return View(model);
            }

            // 2. Generar Código y Guardar en BD (Update)
            string codigo = new Random().Next(100000, 999999).ToString();

            policia.CodVerificacionEmail = codigo;
            policia.FechaExpiracionCod = DateTime.UtcNow.AddMinutes(15);

            await _personalService.ActualizarPolicia(policia);

            // 3. Enviar Correo
            await _emailService.EnviarCorreoVerificacion(policia.EmailInstitucional, policia.Nombres, codigo);

            // 4. Redirigir al Paso 2 (Llevando el email)
            return RedirectToAction("CambiarPasswordPolicia", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult CambiarPasswordPolicia(string email)
        {
            // Preparamos el modelo con el email recibido para que el usuario no tenga que escribirlo de nuevo
            var model = new RestablecerPasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPasswordPolicia(RestablecerPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var policia = await _personalService.ObtenerPorEmailOCodigo(model.Email);

            // Validaciones
            if (policia == null ||
                policia.CodVerificacionEmail != model.Codigo ||
                policia.FechaExpiracionCod < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "El código es inválido o ha expirado.");
                return View(model);
            }

            // CAMBIO DE CONTRASEÑA
            policia.ContrasenaHash = Utilidades.EncriptarClave(model.NuevaPassword);

            // Limpiamos el código usado
            policia.CodVerificacionEmail = null;
            policia.FechaExpiracionCod = null;

            await _personalService.ActualizarPolicia(policia);

            TempData["MensajeExito"] = "Contraseña actualizada. Inicie sesión.";
            return RedirectToAction("LoginPolicia");
        }
    }
}