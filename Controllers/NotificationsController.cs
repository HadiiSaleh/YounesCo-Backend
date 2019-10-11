using Microsoft.AspNetCore.Mvc;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        public IActionResult EmailConfirmed(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                return Redirect("/login");

            }

            return RedirectToPage("Pages/Notifications/EmailConfirmed.cshtml");
        }

        public IActionResult PasswordSet(string id, string code)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(code))
            {
                return Redirect("/login");

            }

            return RedirectToPage("Pages/Notifications/PasswordSet.cshtml");
        }
    }
}