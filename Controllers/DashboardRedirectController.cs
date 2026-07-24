using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorBooking.Data;

namespace TailorBooking.Controllers;

[Authorize]
public class DashboardRedirectController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole(RoleNames.Admin))
        {
            return RedirectToAction(
                "Index",
                "Dashboard",
                new { area = "Admin" });
        }

        if (User.IsInRole(RoleNames.Customer))
        {
            return RedirectToAction(
                "Index",
                "Dashboard",
                new { area = "Customer" });
        }

        return RedirectToAction("Index", "Home");
    }
}