using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicApp.Controllers
{
    [Authorize]
    public class RedirectController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RedirectController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
                return RedirectToAction("Index", "Doctor");

            return RedirectToAction("Index", "Booking");
        }
    }
}