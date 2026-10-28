using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Identity.Controllers
{
    // Placeholder controller — the real Settings module logic will be built
    // separately. This just wires the sidebar link to a live, authorized page.
    [Authorize]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.ModuleName = "Settings";
            return View();
        }
    }
}
