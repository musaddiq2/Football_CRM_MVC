using Microsoft.AspNetCore.Mvc;

namespace FootBallOne.Controllers
{
    public class CampController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
