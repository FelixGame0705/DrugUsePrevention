using Microsoft.AspNetCore.Mvc;

namespace DrugUsePrevention.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            return null;
        }
    }
}
