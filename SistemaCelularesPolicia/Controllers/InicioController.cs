using Microsoft.AspNetCore.Mvc;

namespace SistemaCelularesPolicia.Controllers
{
    public class InicioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
