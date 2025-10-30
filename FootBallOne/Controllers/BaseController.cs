using Microsoft.AspNetCore.Mvc;

namespace FootBallOne.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
