using Microsoft.AspNetCore.Mvc;

namespace FootBallOne.Controllers
{
    public class NotificationTemplates : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
