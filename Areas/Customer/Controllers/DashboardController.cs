using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TailorBooking.Data;

namespace TailorBooking.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = RoleNames.Customer)]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}